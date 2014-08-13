using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Catalogue.Data;
using Catalogue.Web.Logging;
using Newtonsoft.Json.Serialization;
using Raven.Client;

namespace Catalogue.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IDocumentStore DocumentStore { get; private set; }


        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
            filters.Add(new HandleErrorAttribute());
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ConfigWebApi(GlobalConfiguration.Configuration);
            RegisterRoutes(RouteTable.Routes);
            // serialize enums using their strings, not ordinals
//            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            // we want to serialize PascalCase .NET properties with camelCase in json responses
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            InitializeDataStore();
            RegisterGlobalFilters(GlobalFilters.Filters);
        }

        private static void ConfigWebApi(HttpConfiguration config)
        {
            /*
           * this allows only request method type per controller
           */
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new {id = RouteParameter.Optional}
                );
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
                DocumentStore = DatabaseFactory.Create(DatabaseFactory.DatabaseConnectionType.InMemory);
            }
            else
            {
                // todo enable versioning for non-dev environments
                // todo (extract the versioning configuration from the InMemoryDatabaseHelper somehow)
                DocumentStore = DatabaseFactory.Create(DatabaseFactory.DatabaseConnectionType.Proper);
            }
        }
    }
}