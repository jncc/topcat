using System.Security.Principal;
using System.Web;
using Catalogue.Web.Account;
//using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Web.Common;
using Raven.Client.Documents.Session;
using Raven.Client.Documents;
using Catalogue.Web.Code;
using Catalogue.Data.Write;
using System;
using Raven.Embedded;

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
            //this.Bind(x => x
            //    .FromAssembliesMatching("Catalogue.*")
            //    .SelectAllClasses()
            //    .BindDefaultInterface());

            Bind<IEnvironment>().To<Code.Environment>();
            Bind<ISettings>().To<Settings>();
            Bind<IRecordService>().To<RecordService>();
            Bind<IRecordValidator>().To<RecordValidator>();
            Bind<IOpenDataPublishingRecordService>().To<OpenDataPublishingRecordService>();
            Bind<IOpenDataPublishingUploadRecordService>().To<OpenDataPublishingUploadRecordService>();

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
