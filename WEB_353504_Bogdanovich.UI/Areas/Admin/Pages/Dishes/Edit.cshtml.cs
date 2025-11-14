using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.UI.Services.CategoryService; // 🛑 ДОБАВЛЕНО
using Microsoft.AspNetCore.Http; // Для IFormFile
using WEB_353504_Bogdanovich.Domain.Models; // Для ResponseData

namespace WEB_353504_Bogdanovich.UI.Areas.Admin.Pages.Dishes
{
    public class EditModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService; // НОВОЕ ПОЛЕ

        public EditModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService; // ИНИЦИАЛИЗАЦИЯ
        }

        [BindProperty]
        public Dish Dish { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Загрузка блюда
            var response = await _productService.GetProductByIdAsync(id);
            if (!response.Successfull || response.Data == null)
                return NotFound();

            Dish = response.Data;

            // 2. 🛑 ЗАГРУЗКА КАТЕГОРИЙ 🛑
            await LoadCategoriesAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // На случай ошибки валидации или API-ошибки, категории должны быть загружены
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(); // ЗАГРУЗКА ПРИ ОШИБКЕ ВАЛИДАЦИИ
                return Page();
            }

            var response = await _productService.UpdateProductAsync(Dish.Id, Dish, ImageFile);
            if (response.Successfull)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.ErrorMessage ?? "Ошибка обновления");

            await LoadCategoriesAsync(); 
            return Page();
        }

        private async Task LoadCategoriesAsync()
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();

            if (categoriesResponse.Successfull && categoriesResponse.Data != null)
            {

                ViewData["Categories"] = categoriesResponse.Data;
            }
            else
            {
                ViewData["Categories"] = new List<Category>();
                ModelState.AddModelError(string.Empty, categoriesResponse.ErrorMessage ?? "Не удалось загрузить категории.");
            }
        }
    }
}