using System.Configuration;
using System.Web.Http;
using Microsoft.Practices.Unity;
using MongoDB.Driver;
using Zabbkit.Web.Models;
using Zabbkit.Web.Services;

namespace Zabbkit.Web
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            GlobalConfiguration.Configuration.DependencyResolver = new Unity.WebApi.UnityDependencyResolver(container);
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // e.g. container.RegisterType<ITestService, TestService>();      

            //Register MongoDB services
            var dbUrl = MongoUrl.Create(ConfigurationManager.ConnectionStrings["mongo"].ConnectionString);
            var dbClent = new MongoClient(dbUrl);
            var dbServer = dbClent.GetServer();
            var db = dbServer.GetDatabase(dbUrl.DatabaseName);
            container.RegisterInstance(dbServer);
            container.RegisterInstance(db);
            container.RegisterInstance(db.GetCollection<Device>("devices"));
            container.RegisterInstance(db.GetCollection<TrackingRecord>("track"));


            //Register push notification services
            //container.RegisterType<PushBroker, PushBroker>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITrackingService, TrackingService>(new ContainerControlledLifetimeManager());
            container.RegisterType<INotificationService, NotificationService>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDeviceService, DeviceService>(new ContainerControlledLifetimeManager());
            //container.RegisterType<IGaTracker, GaTracker>(new ContainerControlledLifetimeManager());
            return container;
        }
    }
}