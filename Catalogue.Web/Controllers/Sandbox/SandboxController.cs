using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace Catalogue.Web.Controllers.Sandbox
{
    public class SandboxController : Controller
    {
        public ActionResult Active()
        {
            string username = HttpContext.User.Identity.Name;
            string domain = "jncc-boss.green.jncc.gov.uk";


            var domainContext = new PrincipalContext(ContextType.Domain, domain);
            var user = UserPrincipal.FindByIdentity(domainContext, username);
            string email = user.EmailAddress;

            // Add the "proxyaddresses" entries.
            var properties = ((DirectoryEntry)user.GetUnderlyingObject()).Properties;

            var emails = from object property in properties["proxyaddresses"]
                         select property.ToString();

            return Json(new { user.EmailAddress, user.DisplayName , user.GivenName }, JsonRequestBehavior.AllowGet);
        }
    }
//
//    public static class AccountManagementExtensions
//    {
//
//        public static String GetProperty(this IPrincipal principal, String property)
//        {
//            DirectoryEntry directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
//            if (directoryEntry.Properties.Contains(property))
//                return directoryEntry.Properties[property].Value.ToString();
//            else
//                return String.Empty;
//        }
//
//        public static String GetCompany(this IPrincipal principal)
//        {
//            return principal.GetProperty("company");
//        }
//
//        public static String GetDepartment(this IPrincipal principal)
//        {
//            return principal.GetProperty("department");
//        }
//
//    }
}