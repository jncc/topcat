using System.Net.Http;
using Catalogue.Robot.Publishing;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Quartz.Spi;
using Raven.Client.Documents;

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

            Bind<Env>().ToMethod(x => new Env());
            Bind<IDocumentStore>().ToMethod(x => Program.DocumentStore);
            Bind<HttpClient>().ToSelf().InSingletonScope();
            
            //             may want to use the log4net logger
            //            Kernel.Bind<ILog>().ToMethod(x => LogManager.GetLogger(typeof(Program)));

            Bind<IJobFactory>().To<NinjectJobFactory>();
        }
    }
}
