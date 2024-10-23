using Microsoft.AspNetCore.Mvc;
using WebNet48Core;

namespace WebNet48.Controllers
{
    public class HomeController : Controller
    {
        


        public async Task<ActionResult> Index()
        {
            SessionHelper sessionHelper = new SessionHelper(new RedisSessionManager(), HttpContext);

            await sessionHelper.SetAsync("LastVisit", DateTime.UtcNow.ToString());
            await sessionHelper.SetAsync("Test", $"Last Executed on net8.0 at { DateTime.Now }");

            ViewBag.Test = await sessionHelper.GetAsync("Test");

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TestPost()
        {
            ViewBag.Message = "Your contact page.";

            return Json(new { result = "OK" });
        }
    }
}