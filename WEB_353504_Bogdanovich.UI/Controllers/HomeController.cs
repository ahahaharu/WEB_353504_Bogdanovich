using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_353504_Bogdanovich.UI.Models;

namespace WEB_353504_Bogdanovich.UI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Lab"] = "Лабораторная работа №2";
            var list = new List<ListDemo>
            {
                new ListDemo { Id = 1, Name = "Item 1" },
                new ListDemo { Id = 2, Name = "Item 2" },
                new ListDemo { Id = 3, Name = "Item 3" }
            };
            return View(new SelectList(list, "Id", "Name")); 
        }
    }
}
