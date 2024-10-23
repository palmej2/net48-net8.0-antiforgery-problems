using System.Threading.Tasks;
using System.Web.Mvc;

namespace WebNet48.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> Test()
        {
            SessionHelper sessionHelper = new SessionHelper(new RedisSessionManager(), System.Web.HttpContext.Current);
            ViewBag.Test = await sessionHelper.GetAsync("Test");

            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}