using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Routing;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Catalogue.Data.Test;
using Catalogue.Web.Admin;
using Catalogue.Web.Logging;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Database.Server;

namespace Catalogue.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IDocumentStore DocumentStore { get; private set; }
        
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterHttpFilters(HttpFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
        }

        /*public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new ElmahHandledErrorLoggerFilter());
            filters.Add(new HandleErrorAttribute());
        
        }*/
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
            RegisterHttpFilters(GlobalConfiguration.Configuration.Filters);
        }

        static void ConfigWebApi(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            // Controllers with Actions
            // To handle routes like `/api/Admin/import`
           /* config.Routes.MapHttpRoute(
                name: "ControllerAndAction",
                routeTemplate: "api/{controller}/{action}"
            );*/
        }

        static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }

        static void InitializeDataStore()
        {
            if (ConfigurationManager.AppSettings["Environment"] == "Dev")
            {
                // use in-memory database for development

                DocumentStore = new InMemoryDatabaseHelper
                    {
                        PreInitializationAction = store =>
                            {
                                const int port = 8888;
                                store.Configuration.Port = port;
                                NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);
                                store.UseEmbeddedHttpServer = true;
                            },
                        PostInitializationAction = Seeder.Seed
                    }.Create();
            }
            else
            {
                // todo enable versioning for non-dev environments
                // todo (extract the versioning configuration from the InMemoryDatabaseHelper somehow)

                DocumentStore = new DocumentStore { ConnectionStringName = "Data" };
                DocumentStore.Initialize();
                IndexCreation.CreateIndexes(typeof(Record).Assembly, DocumentStore);
            }
        }
    }
}