using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using PushSharp.Apple;
using PushSharp.Core;
using Zabbkit.Web.Models;
using log4net;
using log4net.Util;
using Newtonsoft.Json.Linq;
using PushSharp.Google;
using PushSharp.Windows;

namespace Zabbkit.Web.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NotificationService));
        private ApnsServiceBroker apns;
        private GcmServiceBroker gcm;
        private WnsServiceBroker wns;
        private readonly ITrackingService trackingService;
        private readonly IDeviceService deviceService;

        public NotificationService(ITrackingService trackingService, IDeviceService deviceService)
        {
            this.trackingService = trackingService;
            this.deviceService = deviceService;
            Log.Info("Start sending channels");
            RegisterGcmChannel();
            RegisterApnsChannel();
            RegisterWnsChannel();
        }

        private void RegisterGcmChannel()
        {
            try
            {
                var gcmKey = ConfigurationManager.AppSettings["gcmKey"];
                if (!string.IsNullOrEmpty(gcmKey))
                {
                    Log.DebugFormat("Start GCM channel with key {0}", gcmKey.Substring(0, 5));
                    gcm = new GcmServiceBroker(new GcmConfiguration(gcmKey));
                    gcm.OnNotificationFailed += Gcm_OnNotificationFailed;
                    gcm.OnNotificationSucceeded += Gcm_OnNotificationSucceeded;
                    gcm.Start();
                }
                else
                {
                    Log.Warn("GCM channel has not started due to configuration");
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to start GCM channel", e);
                gcm = null;
            }
        }

        private void RegisterApnsChannel()
        {
            try
            {
                Log.DebugFormat("Start APNS channel with certificate {0}", ConfigurationManager.AppSettings["apnsCertificate"]);
                var apnsCerFileName = ConfigurationManager.AppSettings["apnsCertificate"];
                var apnsCerPwd = ConfigurationManager.AppSettings["apnsCertificatePwd"];
                var isProductionCertVal = ConfigurationManager.AppSettings["apnsCertificateProduction"];
                bool isProductionCert;
                if (!bool.TryParse(isProductionCertVal, out isProductionCert))
                {
                    isProductionCert = false;
                    Log.Warn(
                        "apnsCertificateProduction is not set or has incorrect value in web.config. False value is used by default.");
                }

                var appleCert = File.ReadAllBytes(HostingEnvironment.MapPath(apnsCerFileName));
                apns = new ApnsServiceBroker(new ApnsConfiguration(isProductionCert ? ApnsConfiguration.ApnsServerEnvironment.Production : ApnsConfiguration.ApnsServerEnvironment.Sandbox, appleCert, apnsCerPwd));
                apns.OnNotificationFailed += Apns_OnNotificationFailed;
                apns.OnNotificationSucceeded += Apns_OnNotificationSucceeded;
                apns.Start();
            }
            catch (Exception e)
            {
                Log.Error("Failed to start APNS channel", e);
                apns = null;
            }
        }

        private void RegisterWnsChannel()
        {
            try
            {
                Log.Warn("WNS not configured");
                //Log.DebugFormat("Start WNS channel with params {0}", config.WnsPackageName);
                //wns = new WnsServiceBroker(new WnsConfiguration(config.WnsPackageName, config.WnsPackageSecurityParameter, config.WnsClientSecret));
                //wns.OnNotificationFailed += Wns_OnNotificationFailed;
                //wns.OnNotificationSucceeded += Wns_OnNotificationSucceeded;
                //wns.Start();
            }
            catch (Exception e)
            {
                Log.Error("Failed to start WNS channel", e);
                wns = null;
            }
        }

        protected void Apns_OnNotificationSucceeded(ApnsNotification notification)
        {
            Log.DebugFormatExt("APNS success {0}", notification.Tag);
            TrackEvent(notification.Tag, TrackingStatus.Delivered, "Successfully accepted by Apple");
        }

        protected void Apns_OnNotificationFailed(ApnsNotification notification, AggregateException exception)
        {
            Log.ErrorFormat("APNS failed {0}: {1}", notification.Tag, exception);
            var statusMessage = exception == null ? "Unknown error" : exception.Flatten().ToString();
            TrackEvent(notification.Tag, TrackingStatus.Error, statusMessage);

            if (exception != null && exception.InnerExceptions != null)
            {
                foreach (var e in exception.InnerExceptions.OfType<DeviceSubscriptionExpiredException>())
                {
                    ProcessExpiredToken(notification.Tag as string, e);
                }

                var ex = exception.InnerExceptions.FirstOrDefault(e => e is ApnsNotificationException);
                if (ex != null && ((ApnsNotificationException)ex).ErrorStatusCode == ApnsNotificationErrorStatusCode.InvalidToken)
                {
                    Log.Debug("Invalid token detected, do cleanig up");
                    ProcessInvalidToken(notification.Tag as string, ex);
                    TrackEvent(notification.Tag, TrackingStatus.Ignored, "Invalid APNS token, subscription dropped");
                }
            }
        }

        protected void Gcm_OnNotificationSucceeded(GcmNotification notification)
        {
            Log.DebugFormat("GCM success {0}", notification.Tag);
            TrackEvent(notification.Tag, TrackingStatus.Delivered, "Successfully accepted by Google");
        }

        protected void Gcm_OnNotificationFailed(GcmNotification notification, AggregateException exception)
        {
            Log.ErrorFormat("GCM failed {0}: {1}", notification.Tag, exception);
            foreach (var ex in exception.InnerExceptions.OfType<DeviceSubscriptionExpiredException>())
            {
                ProcessExpiredToken(notification.Tag as string, ex);
            }
            var statusMessage = exception.Flatten().ToString();
            TrackEvent(notification.Tag, TrackingStatus.Error, statusMessage);
        }

        private void ProcessExpiredToken(string trackId, Exception exception)
        {
            if (trackId == null)
            {
                Log.Warn("Empty trackId, unable to drop expired device registration");
                return;
            }
            var t = trackingService.Get(trackId);
            if (t != null)
            {
                deviceService.Delete(t.DeviceId);
                Log.InfoFormat("{0} Device registration dropped due to token expiration, original error message: '{1}', deviceId: {2}", t.DeviceType.ToString("G"), exception.Message.Length, t.DeviceId);
            }
        }

        private void ProcessInvalidToken(string trackId, Exception exception)
        {
            if (trackId == null)
            {
                Log.Warn("Empty trackId, unable to drop expired device registration");
                return;
            }
            var t = trackingService.Get(trackId);
            if (t != null)
            {
                deviceService.Delete(t.DeviceId);
                Log.InfoFormat("{0} Device registration dropped due to invalid token, original error message: '{1}', deviceId: {2}", t.DeviceType.ToString("G"), exception.Message.Length, t.DeviceId);
            }
        }

        private void TrackEvent(object tag, TrackingStatus status, string message)
        {
            var eventId = tag as string;
            if (eventId == null)
            {
                Log.WarnFormat("Incorrect tag {0}, can't track event message {1}: {2}", tag, status, message);
                return;
            }
            trackingService.Update(TrackingStatus.Delivered, eventId, message);
        }

        public void SendMessage(Device device, Message message, string trackingId)
        {
            try
            {
                Log.InfoFormat("Starting send message for {0}, trackingId:{1}", device.Type, trackingId);
                switch (device.Type)
                {
                    case DeviceType.iOS:
                        SendApnsMessage(device.Token, message, trackingId);
                        break;
                    case DeviceType.Android:
                        SendGcmMessage(device.Token, message, trackingId);
                        break;
                    case DeviceType.WP:
                        SendWpMessage(device.Token, message, trackingId);
                        break;
                    default:
                        Log.ErrorFormat("Unsupported device type, message ignored. TrackingId:{0}", trackingId);
                        trackingService.Update(TrackingStatus.Ignored, trackingId, "Unsapported device type");
                        return;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Send message failure, trackingId:{0}, error:{1}", trackingId, ex);
                trackingService.Update(TrackingStatus.Error, trackingId, ex.Message);
                return;
            }
            trackingService.Update(TrackingStatus.Processing, trackingId);
        }

        private void SendApnsMessage(string token, Message message, string trackingId)
        {
            if (!IsApnsChannelAccessable)
            {
                Log.Warn("APNS channel is not available, notification skipped");
                return;
            }

            Log.DebugFormat("Send APNS message to token:{0}", token);
            var payload = new JObject
            {
                {"aps", new JObject
                    {
                        {"alert", message.Text},
                        {"sound", "sound.caf"},
                    }
                },
                {"tid",  message.TriggerId}
            };

            apns.QueueNotification(new ApnsNotification(token)
            {
                Tag = trackingId,
                Payload = payload
            });
        }

        private void SendGcmMessage(string token, Message message, string trackingId)
        {
            if (!IsGcmChannelAccessable)
            {
                Log.Warn("GCM channel is not available, notification skipped");
                return;
            }

            Log.DebugFormat("Send GCM message to token:{0}", token);
            dynamic payload = new JObject();
            payload.triggerId = message.TriggerId;
            //payload.Text = message.Text;
            payload.msg = message.Text;
            payload.sound = message.PlaySound;
            Log.DebugFormat("Notification JSON: {0}", payload);
            gcm.QueueNotification(new GcmNotification
            {
                To = token,
                Data = payload,
                Tag = trackingId
            });
        }

        private void SendWpMessage(string token, Message message, string trackingId)
        {
            if (!IsWpChannelAccessable)
            {
                Log.Warn("WNS channel is not available, notification skipped");
                return;
            }

            //var toastMessage = new WindowsPhoneToastNotification()
            //    .ForEndpointUri(new Uri(token))
            //    .ForOSVersion(WindowsPhoneDeviceOSVersion.MangoSevenPointFive)
            //    .WithBatchingInterval(BatchingInterval.Immediate)
            //    .WithText2(message.Text)
            //    .WithNavigatePath(string.Format("/Views/LoginPage.xaml?triggerIdFromToast={0}", message.TriggerId))
            //    .WithTag(trackingId);
            //var tileMessage = new WindowsPhoneTileNotification()
            //    .ForEndpointUri(new Uri(token))
            //    .ForOSVersion(WindowsPhoneDeviceOSVersion.MangoSevenPointFive)
            //    .WithBatchingInterval(BatchingInterval.Immediate)
            //    .WithBackContent(message.Text)
            //    .WithBackTitle("Zabbkit")
            //    .WithTag(trackingId);

            //_trackingService.Update(TrackingStatus.Processing, trackingId);
            //SendWp(toastMessage);
            //SendWp(tileMessage);
        }

        private void SendWp<T>(T pushMessage) where T : WnsNotification
        {
            Log.Debug("Send WP message");
            wns.QueueNotification(pushMessage);
        }

        public bool IsApnsChannelAccessable => apns != null;

        public bool IsGcmChannelAccessable => gcm != null;

        public bool IsWpChannelAccessable => wns != null;
    }
}