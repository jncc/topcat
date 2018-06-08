using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Routing;
using Catalogue.Data;
using Catalogue.Web.Customization;
using Catalogue.Web.Logging;
using Newtonsoft.Json.Serialization;
using Raven.Client.Documents;

namespace Catalogue.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IDocumentStore DocumentStore { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(ConfigWebApi);
            RegisterRoutes(RouteTable.Routes);

            // serialize PascalCase .NET properties with camelCase in json
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            InitializeDataStore();
            RegisterGlobalFilters(GlobalFilters.Filters);
        }

        static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
            filters.Add(new HandleErrorAttribute());
        }
        
        static void ConfigWebApi(HttpConfiguration config)
        {
            // we need our own custom assemblies resolver for webapi - see type summary
            config.Services.Replace(typeof(IAssembliesResolver), new CustomWebApiAssembliesResolver());

            // enable attribute-based routing
            config.MapHttpAttributeRoutes();

            // default web api route pattern
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            // what's this for?
            config.Filters.Add(new UnhandledExceptionFilter());
        }

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Default", "{controller}/{action}/{id}",
                new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }

        private static void InitializeDataStore()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Dev")
            {
                // use in-memory database for development
                DocumentStore = DatabaseFactory.InMemory();
            }
            else
            {
                // todo enable versioning for non-dev environments
                // todo (extract the versioning configuration from the InMemoryDatabaseHelper somehow)
                DocumentStore = DatabaseFactory.Production();
            }
        }
    }
}