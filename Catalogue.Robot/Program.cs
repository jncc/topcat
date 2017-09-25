using Catalogue.Data;
using Catalogue.Robot.Injection;
using Catalogue.Robot.Publishing.OpenData;
using log4net;
using log4net.Config;
using Ninject;
using Quartz;
using Raven.Client;
using System;
using System.Configuration;
using System.Net.Http;
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

            ILog logger = LogManager.GetLogger(typeof(Program));
            logger.Info("Entry point");

            bool runOnce = args != null && args.Length > 0 && "runOnce".Equals(args[0]);

            if (runOnce)
            {
                Logger.Info("Running upload in runOnce mode");
                var uploadJob = CreateUploadJob();
                uploadJob.RunUpload();
                Logger.Info("Finished run");
            }
            else
            {
                logger.Info("Running service");
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
                            q.WithJob(() => JobBuilder.Create<OpenDataUploadJob>().Build())
                                .AddTrigger(() => TriggerBuilder.Create()
                                    .WithDailyTimeIntervalSchedule(b => b
                                        .WithIntervalInMinutes(1)
                                        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(10, 00)))
                                    .Build()
                                )
                        );
                    });

                    string serviceName = "Topcat.Robot." + ConfigurationManager.AppSettings["Environment"];
                    x.SetDisplayName(serviceName);
                    x.SetServiceName(serviceName);
                    x.SetDescription("Uploads metadata and data files from Topcat to data.jncc.gov.uk");
                });
                logger.Info("Finished running service");
            }
        }

        /// <summary>
        /// Creates an instance with dependecies injected.
        /// </summary>
        public static OpenDataUploadJob CreateUploadJob()
        {
            var kernel = new StandardKernel();

            // register the type bindings we want for injection 
            kernel.Load<MainNinjectModule>();

            return kernel.Get<OpenDataUploadJob>();
        }
    }
}
