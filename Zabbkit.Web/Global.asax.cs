using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Zabbkit.Web.Controllers;
using log4net;

namespace Zabbkit.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private ILog _log;
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            _log = LogManager.GetLogger(typeof(WebApiApplication));
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            Bootstrapper.Initialise();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (_log != null)
            {
                var exception = Server.GetLastError();
                if (exception != null)
                    _log.Error("Application_Error", exception);
            }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            if (_log != null)
            {
                _log.InfoFormat("Application_BeginRequest: {0}", Request.Path);
            }

        }

        protected void Application_EndRequest(Object sender, EventArgs e)
        {
            if (_log != null)
            {
                _log.InfoFormat("Application_EndRequest: {0}", Request.Path);
            }
        }
    }
}