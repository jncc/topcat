using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Catalogue.Web.Code;
using Catalogue.Web.Code.Account;
using Ninject;
using Ninject.Extensions.Conventions;
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
            // use Ninject.Extensions.Conventions for easy ISomeType -> SomeType bindings
            Kernel.Bind(x => x
                .FromAssembliesMatching("Catalogue.*")
                .SelectAllClasses()
                .BindDefaultInterface());

            // the raven document store should be an application-scoped singleton
            Bind<IDocumentStore>().ToMethod(x => WebApiApplication.DocumentStore);

            // inject a once-per-request raven document session
            Bind<IDocumentSession>()
                .ToMethod(x => WebApiApplication.DocumentStore.OpenSession())
                .InRequestScope();

            // convenience binding for the asp.net-provided current user
            // which is used by the once-per-request user context object
            Bind<IPrincipal>().ToMethod(x => HttpContext.Current.User);
            Rebind<IUserContext>().To<UserContext>().InRequestScope();
        }
    }
}
