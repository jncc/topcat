using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using Ninject;

namespace Catalogue.Web.Injection
{
    /// <summary>
    /// This is needed at the moment to hook up ninject with web api v4.
    /// http://www.peterprovost.org/blog/2012/06/19/adding-ninject-to-web-api
    /// </summary>
    public class NinjectDependencyResolver : NinjectScope, IDependencyResolver
    {
        private IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            _kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectScope(_kernel.BeginBlock());
        }
    }
}