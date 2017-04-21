using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zabbkit.Web.Models;
using Zabbkit.Web.Services;
using log4net;

namespace Zabbkit.Web.Controllers
{
    [ApiErrorHandler]
    public class DevicesController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DevicesController));
        private readonly IDeviceService _deviceService;
        private readonly ITrackingService _trackingService;

        public DevicesController(IDeviceService deviceService, ITrackingService trackingService)
        {
            _deviceService = deviceService;
            _trackingService = trackingService;
        }

        // POST api/values
        [ModelValidation]
        public async Task<HttpResponseMessage> Post([FromBody]Device value)
        {
            Log.Info("Add device request from IP: " + Request.GetClientIpAddress());
            using (var gaTracker = _trackingService.StartGaSession(Request, value.Id))
            {
                await gaTracker.TrackPage("Device", "/api/devices");
                await gaTracker.TrackEvent("Device", "Registration");
            }
            value.Id = null;
            value.Token = value.Token.Trim();
            _deviceService.Create(value);
            return Request.CreateResponse(HttpStatusCode.OK, new { value.Id });
        }

        [ModelValidation]
        public async Task<HttpResponseMessage> Put([FromBody] TokenRenewRequest value)
        {
            Log.InfoFormat("Token renew request deviceId: {0}, IP: {1}", value.Id, Request.GetClientIpAddress());
            using (var gaTracker = _trackingService.StartGaSession(Request, value.Id))
            {
                await gaTracker.TrackPage("Device", "/api/devices");
                await gaTracker.TrackEvent("Device", "Update");
            }
            _deviceService.RenewTokent(value);
            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

#if !PRODUCTION
        // GET api/values
        public async Task<IEnumerable<Device>> Get()
        {
            using (var gaTracker = _trackingService.StartGaSession(Request, "anonymous"))
            {
                await gaTracker.TrackPage("Device", "/api/devices");
                await gaTracker.TrackEvent("Device", "Update");
            }
           return _deviceService.GetAll();
        }

        // GET api/values/5
        public HttpResponseMessage Get(string id)
        {
            var device = _deviceService.Get(id);
            return device == null ? Request.CreateErrorResponse(HttpStatusCode.NotFound, "Device not found") : Request.CreateResponse(HttpStatusCode.OK, device);
        }

        // DELETE api/values/5
        public HttpResponseMessage Delete(string id)
        {
            _deviceService.Delete(id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
#endif
    }
}