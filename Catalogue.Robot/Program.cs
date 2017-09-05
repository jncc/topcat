using Catalogue.Data;
using Catalogue.Robot.Injection;
using log4net;
using log4net.Config;
using Ninject;
using Raven.Client;
using System;
using System.Net.Http;
using Topshelf;

namespace Catalogue.Robot
{
    class Program
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Program));

        public static IDocumentStore DocumentStore
        {
            get
            {
                try
                {
                    // DatabaseFactory.Production() returns the RavenDB DocumentStore using the connection
                    // string name specfied in the local web.config or app.config, so this is just what we need
                    // for both production and local dev (where we use the db running in the local dev web app)
                    return DatabaseFactory.Production();
                }
                catch (HttpRequestException ex)
                {
                    var e = new Exception("Unable to connect to the Topcat database.", ex);
                    logger.Error(e);
                    throw e;
                }
            }
        }

        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                XmlConfigurator.Configure();

                x.Service<Robot>(s =>
                {
                    s.ConstructUsing(name => CreateRobot());
                    s.WhenStarted(p => p.Start());
                    s.WhenStopped(p => p.Stop());
                });

                x.RunAsLocalSystem();

                string nombre = "Topcat.Robot." + "TODO"; // settings.Environment;
                x.SetDisplayName(nombre);
                x.SetServiceName(nombre);
                x.SetDescription("Description of Robot");
            });
        }

        /// <summary>
        /// Creates an instance with dependecies injected.
        /// </summary>
        public static Robot CreateRobot()
        {
            var kernel = new StandardKernel();

            // register the type bindings we want for injection 
            kernel.Load<MainNinjectModule>();

            return kernel.Get<Robot>();
        }
    }
}
