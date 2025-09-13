using Microsoft.AspNetCore.Mvc;

namespace WEB_353504_Bogdanovich.UI.Components
{
    public class Cart : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(); 
        }
    }
}
