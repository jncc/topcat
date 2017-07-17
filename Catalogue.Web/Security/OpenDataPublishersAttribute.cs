
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http;

namespace Catalogue.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method )]
    public class OpenDataPublishersAttribute : AuthorizeAttribute
    {
        public OpenDataPublishersAttribute()
        {
            Roles = "Publisher";
        }

//        protected override bool AuthorizeCore(HttpContextBase httpContext)
//        {
//            if (httpContext.Request.IsLocal)
//            {
//                // Authorise local requests for dev
//                return true;
//            }
//
//            return base.AuthorizeCore(httpContext);
//        }
    }
}