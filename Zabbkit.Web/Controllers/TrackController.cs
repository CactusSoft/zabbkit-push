using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Practices.Unity;
using MongoDB.Bson;
using Zabbkit.Web.Models;
using Zabbkit.Web.Services;

namespace Zabbkit.Web.Controllers
{
    [ApiErrorHandler]
    public class TrackController : ApiController
    {
        [Dependency]
        public ITrackingService TrackingService { get; set; }

        // GET api/track/5
        public HttpResponseMessage Get(string id)
        {
            ObjectId.Parse(id); //Validation
            var record = TrackingService.Get(id.Trim());
            if (record == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Record not found");
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                DeviceType = record.DeviceType.ToString(),
                Status = record.Status.ToString(),
                record.Updated,
                record.Description
            });
        }

#if !PRODUCTION
        // GET api/track
        public IEnumerable<TrackingRecord> Get()
        {
            return TrackingService.FindAll();
        }
#endif
    }
}
