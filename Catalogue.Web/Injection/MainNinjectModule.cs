using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
using Raven.Client;

namespace Catalogue.Web.Injection
{
    /// <summary>
    /// Defines the type bindings for dependency injection.
    /// </summary>
    public class MainNinjectModule : NinjectModule
    {
        public override void Load()
        {
            // use Ninject.Extensions.Conventions for easy convention-based binding
            //            this.Kernel.Scan(scanner =>
            //            {
            //                scanner.FromAssembliesMatching("Catalogue.*");
            //                scanner.BindWithDefaultConventions();
            //            });

            //            Bind<ILog>().ToMethod(x => LogManager.GetLogger("Catalogue.Web.log"));

            Bind<IDocumentStore>().ToMethod(x => WebApiApplication.DocumentStore);
            Bind<IDocumentSession>()
                .ToMethod(c => c.Kernel.Get<IDocumentStore>().OpenSession())
                .InRequestScope();

            // convenient and efficient (get user just once)
            //            Rebind<IUserContext>()
            //                .ToMethod(x => new UserContext(Kernel.Get<IDocumentSession>(), Kernel.Get<IAuthenticationService>(), Kernel.Get<ISessionService>()))
            //                .InRequestScope();

            if (ConfigurationManager.AppSettings["Environment"] == "Dev")
            {
                // write emails to the file system
                //                Rebind<ISmtpService>().To<FileSystemSmtpService>();
            }
        }
    }
}
