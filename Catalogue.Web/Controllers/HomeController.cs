using System;
using System.IO;
using System.Web.Mvc;

namespace Catalogue.Web.Controllers.Home
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            throw new Exception();
          //  return RedirectPermanent("~/app");
        }

        
    }
}
