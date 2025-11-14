using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.Domain.Models; // <-- Убедитесь, что этот using есть
using WEB_353504_Bogdanovich.UI.Services.ProductService;

namespace WEB_353504_Bogdanovich.UI.Areas.Admin.Pages.Dishes
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        // 1. Добавляем модель, которая будет содержать данные и пагинацию
        public ListModel<Dish> DishListModel { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int pageNo = 1)
        {
            var response = await _productService.GetProductListAsync(
                categoryNormalizedName: null, 
                pageNo: pageNo 
            );

            if (response.Successfull && response.Data != null)
            {
                DishListModel = response.Data;
            }

            return Page();
        }
    }
}