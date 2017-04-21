using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using log4net;
using Zabbkit.Web.Models;
using Zabbkit.Web.Services;

namespace Zabbkit.Web.Controllers
{
    [ApiErrorHandler]
    public class MessagesController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessagesController));
        private readonly IDeviceService _deviceService;
        private readonly ITrackingService _trackingService;
        private readonly INotificationService _notificationService;

        public MessagesController(ITrackingService trackingService, INotificationService notificationService, IDeviceService deviceService)
        {
            _trackingService = trackingService;
            _notificationService = notificationService;
            _deviceService = deviceService;
        }


        // POST api/messages
        [ModelValidation]
        public async Task<HttpResponseMessage> Post(HttpRequestMessage request, [FromBody]Message value)
        {
            if (!_notificationService.IsApnsChannelAccessable)
                return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "APNS is temporary unavailable");
            if (value.Text != null)
                value.Text = value.Text.Trim();
            if (string.IsNullOrEmpty(value.Text))
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Message is empty");

            //Looking for regidtred devce
            var device = _deviceService.Get(value.Id);
            if (device == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Device not found");

            //Check chanel accesability
            switch (device.Type)
            {
                case DeviceType.Android:
                    if (!_notificationService.IsApnsChannelAccessable)
                        return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "APNS is temporary unavailable");
                    break;
                case DeviceType.iOS:
                    if (!_notificationService.IsGcmChannelAccessable)
                        return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "GCM is temporary unavailable");
                    break;
                case DeviceType.WP:
                    if (!_notificationService.IsWpChannelAccessable)
                        return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "WP is temporary unavailable");
                    break;
                default:
                    return Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, "Device type is not supported");
            }

            var track = new TrackingRecord
            {
                DeviceType = device.Type,
                SenderIp = request.GetClientIpAddress(),
                Status = TrackingStatus.Accepted,
                DeviceId = device.Id
            };
            var trackingId = _trackingService.Create(track);
            using (var gaTracker = _trackingService.StartGaSession(request, value.Id))
            {
                await gaTracker.TrackPage("Message", "/api/messages");
                await gaTracker.TrackEvent("Message", device.Type.ToString());
            }

            _notificationService.SendMessage(device, value, trackingId);
            return String.IsNullOrEmpty(trackingId) ? request.CreateResponse(HttpStatusCode.NoContent) : request.CreateResponse(HttpStatusCode.Accepted, new { trackingId = track.Id });
        }
    }
}
