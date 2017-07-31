using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Raven.Client;

namespace Catalogue.Robot.Injection
{
    /// <summary>
    /// Defines the type bindings for dependency injection.
    /// </summary>
    public class MainNinjectModule : NinjectModule
    {
        public override void Load()
        {
            // use Ninject.Extensions.Conventions for easy ISomeType -> SomeType bindings
            Kernel.Bind(x => x
                .FromAssembliesMatching("Catalogue.*")
                .SelectAllClasses()
                .BindDefaultInterface());

            Bind<IDocumentStore>().ToMethod(x => Program.DocumentStore);

            // may want to use the log4net logger
            //this.Kernel.Bind<ILog>().ToMethod(x => LogManager.GetLogger(typeof(Blah)));

            // may want to use quartz for scheduling; something like this
            //            // bind the quartz scheduler factory to ninject managed version
            //            Bind<ISchedulerFactory>().To<NinjectSchedulerFactory>();
            //            Bind<IScheduler>().ToMethod(ctx => ctx.Kernel.Get<ISchedulerFactory>().GetScheduler()).InSingletonScope();
        }
    }
}
