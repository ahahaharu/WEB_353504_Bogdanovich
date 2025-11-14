using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.Domain.Entities;

namespace WEB_353504_Bogdanovich.UI.Areas.Admin.Pages.Dishes
{
    public class DeleteModel : PageModel
    {
        private readonly IProductService _productService;

        public DeleteModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        public Dish Dish { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var response = await _productService.GetProductByIdAsync(id);
            if (!response.Successfull || response.Data == null)
            {
                var errorMessage = response.ErrorMessage ?? (response.Successfull ? "Данные пусты." : "Ошибка API.");

                ModelState.AddModelError(string.Empty, errorMessage);
                return Page();
            }

            Dish = response.Data;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var response = await _productService.DeleteProductAsync(id);
            if (response.Successfull)
                return RedirectToPage("Index");

            ModelState.AddModelError("", response.ErrorMessage ?? "Ошибка удаления");
            return Page();
        }
    }
}
