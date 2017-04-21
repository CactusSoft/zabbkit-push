using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using MongoDB.Bson;
using log4net;

namespace Zabbkit.Web.Controllers
{
    public class ApiErrorHandlerAttribute : ActionFilterAttribute
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ApiErrorHandlerAttribute));
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;
            if (exception == null || exception.GetType() == typeof(HttpResponseException)) return;
            var status = HttpStatusCode.InternalServerError;
            if (exception.GetType() == typeof(NotImplementedException))
                status = HttpStatusCode.NotImplemented;
            if (exception.GetType() == typeof(FormatException))
                status = HttpStatusCode.BadRequest;
            actionExecutedContext.Response =
                actionExecutedContext.Request.CreateErrorResponse(status, exception.Message);
            Log.WarnFormat("Unhandeled exception {0}:'{1}' is translated to HTTP {2}", exception.GetType(), exception.Message, status);
        }
    }
}