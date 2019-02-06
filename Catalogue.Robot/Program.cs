using Catalogue.Data;
using Catalogue.Robot.Injection;
using log4net;
using log4net.Config;
using Ninject;
using Quartz;
using System;
using System.Configuration;
using System.Net.Http;
using Catalogue.Robot.Publishing;
using Raven.Client.Documents;
using Topshelf;
using Topshelf.Ninject;
using Topshelf.Quartz;

namespace Catalogue.Robot
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

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
                    Logger.Error(e);
                    throw e;
                }
            }
        }

        private static void Main(string[] args)
        {
            GlobalContext.Properties["LogFileName"] = ConfigurationManager.AppSettings["LogFilePath"];
            XmlConfigurator.Configure();

            bool runOnce = args != null && args.Length > 0 && "runOnce".Equals(args[0]);

            if (runOnce)
            {
                bool metadataOnly = args.Length > 1 && "metadataOnly".Equals(args[1]);
                Logger.Info("Running upload in runOnce mode, metadataOnly=" + metadataOnly);

                var uploadJob = CreateUploadJob();
                uploadJob.RunUpload(metadataOnly);
                Logger.Info("Finished run");
            }
            else
            {
                Logger.Info("Running scheduled service");
                var startHour = Convert.ToInt32(ConfigurationManager.AppSettings["UploaderStartHour"]);
                var startMin = Convert.ToInt32(ConfigurationManager.AppSettings["UploaderStartMinute"]);
                var interval = Convert.ToInt32(ConfigurationManager.AppSettings["UploaderRunIntervalInHours"]);

                HostFactory.Run(x =>
                {
                    x.UseNinject(new MainNinjectModule());
                    x.UsingQuartzJobFactory(() => new NinjectJobFactory(NinjectBuilderConfigurator.Kernel));

                    x.Service<Robot>(s =>
                    {
                        s.ConstructUsingNinject();
                        s.WhenStarted(p => p.Start());
                        s.WhenStopped(p => p.Stop());
                        s.ScheduleQuartzJob(q =>
                            q.WithJob(() => JobBuilder.Create<UploadJob>().Build())
                                .AddTrigger(() => TriggerBuilder.Create()
                                    .WithDailyTimeIntervalSchedule(b => b
                                        .WithIntervalInHours(interval)
                                        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(startHour, startMin)))
                                    .Build()
                                )
                        );
                    });

                    string serviceName = "Topcat.Robot." + ConfigurationManager.AppSettings["Environment"];
                    x.SetDisplayName(serviceName);
                    x.SetServiceName(serviceName);
                    x.SetDescription("Uploads metadata and data files from Topcat to data.jncc.gov.uk");
                });
            }
        }

        /// <summary>
        /// Creates an instance with dependecies injected.
        /// </summary>
        public static UploadJob CreateUploadJob()
        {
            var kernel = new StandardKernel();

            // register the type bindings we want for injection 
            kernel.Load<MainNinjectModule>();

            return kernel.Get<UploadJob>();
        }
    }
}
