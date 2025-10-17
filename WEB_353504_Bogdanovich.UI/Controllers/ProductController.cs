using Microsoft.AspNetCore.Mvc;

namespace WEB_353504_Bogdanovich.UI.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string? category, int pageNo = 1)
        {
            var productResponse = await _productService.GetProductListAsync(category, pageNo);
            if (!productResponse.Successfull)
            {
                return NotFound(productResponse.ErrorMessage);
            }

            var categoriesResponse = await _categoryService.GetCategoryListAsync();
            if (!categoriesResponse.Successfull)
            {
                return NotFound(categoriesResponse.ErrorMessage);
            }

            ViewData["categories"] = categoriesResponse.Data;
            ViewData["currentCategory"] = category == null ? "Все" : categoriesResponse.Data.FirstOrDefault(c => c.NormalizedName == category)?.Name ?? "Все";

            ViewBag.ReturnUrl = Request.Path + Request.QueryString.ToUriComponent();

            return View(productResponse.Data);
        }
    }
}
