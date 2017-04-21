using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Web;

namespace Zabbkit.Web.Controllers
{
    public static class HttpRequestMessageHelper
    {
        public static string GetClientIpAddress(this HttpRequestMessage request)
        {
            if (request.Headers.Contains("X-Forwarded-For"))
            {
                var xHeader = request.Headers.GetValues("X-Forwarded-For").First();
                if (!String.IsNullOrEmpty(xHeader))
                    return xHeader.Split(',').First();
            }
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var prop = (RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name];
                return prop.Address;
            }
            return null;
        }
    }
}