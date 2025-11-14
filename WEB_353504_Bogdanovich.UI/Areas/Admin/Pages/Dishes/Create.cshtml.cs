using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.UI.Services.CategoryService;
using WEB_353504_Bogdanovich.UI.Services.ProductService;

namespace WEB_353504_Bogdanovich.UI.Areas.Admin.Pages.Dishes
{
    public class CreateModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public CreateModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        [BindProperty]
        public Dish Dish { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGet()
        {
            await LoadCategories();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return Page();
            }

            var response = await _productService.CreateProductAsync(Dish, ImageFile);

            if (response.Successfull)
                return RedirectToPage("Index");

            await LoadCategories();
            ModelState.AddModelError("", response.ErrorMessage ?? "Ошибка создания");
            return Page();
        }

        private async Task LoadCategories()
        {
            var response = await _categoryService.GetCategoryListAsync();

            if (response.Successfull && response.Data != null)
            {
                // ResponseData.Data используется напрямую, что решает ошибку CS1061
                ViewData["Categories"] = response.Data;
            }
            else
            {
                ViewData["Categories"] = new List<Category>();
            }
        }
    }
}