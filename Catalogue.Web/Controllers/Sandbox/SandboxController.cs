using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catalogue.Web.Controllers.Sandbox
{
    public class SandboxController : Controller
    {
        public ActionResult Index()
        {
            return Json(HttpContext.User.Identity.Name, JsonRequestBehavior.AllowGet);
        }
    }
}