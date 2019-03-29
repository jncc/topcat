using Catalogue.Robot.Injection;
using Catalogue.Robot.Publishing;
using log4net;
using log4net.Config;
using Ninject;
using Quartz;
using System;
using System.Configuration;
using Topshelf;
using Topshelf.Ninject;
using Topshelf.Quartz;

namespace Catalogue.Robot
{

    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            GlobalContext.Properties["LogFileName"] = ConfigurationManager.AppSettings["LogFilePath"];
            XmlConfigurator.Configure();

            bool runOnce = args != null && args.Length > 0 && "runOnce".Equals(args[0]);

            if (runOnce)
            {
                bool metadataOnly = args.Length > 1 && "metadataOnly".Equals(args[1]);
                Logger.Info("Running unscheduled publish, metadataOnly=" + metadataOnly);
                var uploadJob = CreateUploadJob();
                uploadJob.Publish(metadataOnly);
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
                            q.WithJob(() => JobBuilder.Create<PublishingJob>().Build())
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
        /// Creates an instance with dependencies injected.
        /// </summary>
        public static PublishingJob CreateUploadJob()
        {
            var kernel = new StandardKernel();

            // register the type bindings we want for injection 
            kernel.Load<MainNinjectModule>();

            return kernel.Get<PublishingJob>();
        }
    }
}
