using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Dispatcher;

namespace Catalogue.Web.Customization
{
    /// <summary>
    /// Restricts the assemblies considered when webapi looks for controllers.
    /// This is to workaround the problem of controllers/routes clashing with RavenDB
    /// which also uses webapi.
    /// </summary>
    public class CustomWebApiAssembliesResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            return new [] { typeof(CustomWebApiAssembliesResolver).Assembly };
        }
    }
}