using Microsoft.AspNetCore.Mvc;

namespace WEB_353504_Bogdanovich.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
