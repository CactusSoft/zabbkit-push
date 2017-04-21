using System.Collections.Generic;
using System.Net.Http;
using Zabbkit.Web.Models;

namespace Zabbkit.Web.Services
{
    public interface ITrackingService
    {
        string Create(TrackingRecord record);
        void Update(TrackingStatus status, string trackingId, string message = null);
        IGaTracker StartGaSession(HttpRequestMessage request, string userId);
        TrackingRecord Get(string id);
        IEnumerable<TrackingRecord> FindAll();
    }
}
