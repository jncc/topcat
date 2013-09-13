using System.Web.Mvc;

namespace Catalogue.Web.Controllers.Home
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return RedirectPermanent("~/app");
        }
    }
}
