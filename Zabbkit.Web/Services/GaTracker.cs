using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using GoogleAnalyticsTracker.Core.TrackerParameters;
using GoogleAnalyticsTracker.WebAPI2;
using log4net;

namespace Zabbkit.Web.Services
{
    public class GaTracker : IGaTracker
    {
        private readonly HttpRequestMessage request;
        private static readonly ILog Log = LogManager.GetLogger(typeof(GaTracker));
        private readonly Tracker tracker;
        private readonly string userAgent;

        public GaTracker(HttpRequestMessage request)
        {
            this.request = request;
            tracker = new Tracker(ConfigurationManager.AppSettings["gaId"], ConfigurationManager.AppSettings["gaHost"]);
            userAgent = request.Headers.UserAgent.ToString();
        }

        public async Task TrackPage(string pageTitle, string pageUrl)
        {
            Log.Debug("Track page");
            await tracker.TrackPageViewAsync(request, pageTitle, pageUrl);
        }

        public async Task TrackEvent(string category, string action)
        {
            Log.Debug("Track event");
            await tracker.TrackAsync(new EventTracking {Action = action,Category = category,UserAgent = userAgent});
        }
        
        public void Dispose()
        {
            tracker?.Dispose();
        }
    }
}