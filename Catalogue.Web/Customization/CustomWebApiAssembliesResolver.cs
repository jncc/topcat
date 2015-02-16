using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http.Dispatcher;

namespace Catalogue.Web.Customization
{
    /// <summary>
    /// Restricts the assemblies available to webapi when searching for controllers to just this assembly.
    /// Avoids clashing with controllers in RavenDB assemblies.
    /// </summary>
    public class CustomWebApiAssembliesResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            return new [] { typeof(CustomWebApiAssembliesResolver).Assembly };
        }
    }
}