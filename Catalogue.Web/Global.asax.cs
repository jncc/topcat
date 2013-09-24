using System.Configuration;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using Newtonsoft.Json.Serialization;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Database.Server;

namespace Catalogue.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IDocumentStore DocumentStore { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            ConfigWebApi(GlobalConfiguration.Configuration);
            RegisterRoutes(RouteTable.Routes);

            // we want to serialize PascalCase .NET properties with camelCase in json responses
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();

            InitializeDataStore();
        }

        static void ConfigWebApi(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
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
                var s = new EmbeddableDocumentStore();
                const int port = 8888;
                s.Configuration.Port = port;
                NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(port);
                s.RunInMemory = true;
                s.UseEmbeddedHttpServer = true;
                s.Initialize();
                // seed the database with dev-time data
                Seeder.Seed(s);

                DocumentStore = s;
            }
            else
            {
                DocumentStore = new DocumentStore { ConnectionStringName = "Data" };
                DocumentStore.Initialize();
            }

            Raven.Client.Indexes.IndexCreation.CreateIndexes(typeof(Record).Assembly, DocumentStore);
        }
    }
}