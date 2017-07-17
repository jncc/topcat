
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Catalogue.Web.Security
{
    public class OpenDataPublishersAttribute : AuthorizeAttribute
    {
        public OpenDataPublishersAttribute()
        {
            var allRoles = (NameValueCollection)ConfigurationManager.GetSection("roles");
            Roles = allRoles["OpenDataPublishers"];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext.Request.IsLocal)
            {
                // Authorise local requests for dev
                return true;
            }

            return httpContext.Request.IsLocal || base.AuthorizeCore(httpContext);
        }
    }
}