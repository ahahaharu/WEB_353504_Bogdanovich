using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.UI.Services.CategoryService; // 🛑 ДОБАВЛЕНО
using WEB_353504_Bogdanovich.Domain.Models;

namespace WEB_353504_Bogdanovich.UI.Areas.Admin.Pages.Dishes
{
    public class DetailsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService; // 🛑 ДОБАВЛЕНО

        // 🛑 ИЗМЕНЕНИЕ КОНСТРУКТОРА 🛑
        public DetailsModel(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService; // 🛑 ИНИЦИАЛИЗАЦИЯ
        }

        public Dish Dish { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // 1. Загрузка блюда
            var response = await _productService.GetProductByIdAsync(id);
            if (!response.Successfull || response.Data == null)
            {
                // На всякий случай логируем, если данных нет
                ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Блюдо не найдено.");
                return NotFound();
            }

            Dish = response.Data;

            // 2. 🛑 ЗАГРУЗКА КАТЕГОРИЙ 🛑
            await LoadCategoryNameAsync();

            return Page();
        }

        // 🛑 НОВЫЙ МЕТОД: ЗАГРУЗКА ИМЕНИ КАТЕГОРИИ 🛑
        private async Task LoadCategoryNameAsync()
        {
            var categoriesResponse = await _categoryService.GetCategoryListAsync();

            if (categoriesResponse.Successfull && categoriesResponse.Data != null)
            {
                // Находим нужную категорию по CategoryId, полученному вместе с Dish
                var category = categoriesResponse.Data
                                                 .FirstOrDefault(c => c.Id == Dish.CategoryId);

                if (category != null)
                {
                    Dish.Category = new Category { Name = category.Name };
                }
            }
        }
    }
}