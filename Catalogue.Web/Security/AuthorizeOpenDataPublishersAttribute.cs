using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Catalogue.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeOpenDataPublishersAttribute : AuthorizeAttribute
    {
        public AuthorizeOpenDataPublishersAttribute()
        {
            var allRoles = (NameValueCollection) ConfigurationManager.GetSection("roles");
            Roles = allRoles["OpenDataPublishers"];
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // Authorise local requests for dev
            return actionContext.Request.IsLocal() || base.IsAuthorized(actionContext);
        }
    }
}