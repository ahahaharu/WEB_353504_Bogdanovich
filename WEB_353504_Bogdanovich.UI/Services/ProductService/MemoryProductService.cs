using Microsoft.Extensions.Configuration;  
using System.IO;

namespace WEB_353504_Bogdanovich.UI.Services.ProductService
{
    public class MemoryProductService : IProductService
    {
        private List<Dish> _dishes;
        private List<Category> _categories;
        private readonly IConfiguration _config;

        public MemoryProductService(ICategoryService categoryService, IConfiguration config)
        {
            _config = config;
            _categories = categoryService.GetCategoryListAsync().Result.Data;
            SetupData();
        }

        private void SetupData()
        {
            _dishes = new List<Dish>
{
    new Dish { Id = 1, Name = "Суп-харчо", Description = "Очень острый, невкусный", Calories = 200, Image = "images/Sup.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 2, Name = "Борщ", Description = "Много сала, без сметаны", Calories = 330, Image = "images/Borsch.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 3, Name = "Куриный бульон", Description = "С фрикадельками, по бабушкиному рецепту", Calories = 450, Image = "images/Kuriniy_bulon.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 4, Name = "Шурпа", Description = "Дичь какая-то", Calories = 300, Image = "images/Shurpa.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 5, Name = "Тыквенный суп", Description = "Со сливками", Calories = 340, Image = "images/Tikvenniy_sup.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 6, Name = "Солянка", Description = "Сборная", Calories = 400, Image = "images/Solyanka.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("soups")) },
    new Dish { Id = 7, Name = "Плов", Description = "С бараниной и специями", Calories = 600, Image = "images/Plov.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("main-dishes")) },
    new Dish { Id = 8, Name = "Котлета по-киевски", Description = "С маслом внутри", Calories = 550, Image = "images/Kotleta_kievskaya.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("main-dishes")) },
    new Dish { Id = 9, Name = "Жареная картошка", Description = "С луком и грибами", Calories = 480, Image = "images/Zhar_kartoshka.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("main-dishes")) },
    new Dish { Id = 10, Name = "Чизкейк", Description = "Нью-Йоркский стиль", Calories = 400, Image = "images/Cheesecake.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("desserts")) },
    new Dish { Id = 11, Name = "Тирамису", Description = "С кофе и маскарпоне", Calories = 350, Image = "images/Tiramisu.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("desserts")) },
    new Dish { Id = 12, Name = "Медовик", Description = "С кремом из сгущёнки", Calories = 450, Image = "images/Medovik.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("desserts")) },
    new Dish { Id = 13, Name = "Оливье", Description = "Классический, с колбасой", Calories = 300, Image = "images/Olivie.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("salads")) },
    new Dish { Id = 14, Name = "Цезарь", Description = "С курицей и пармезаном", Calories = 320, Image = "images/Caesar.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("salads")) },
    new Dish { Id = 15, Name = "Греческий", Description = "С фетой и оливками", Calories = 250, Image = "images/Greek_salad.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("salads")) },
    new Dish { Id = 16, Name = "Чай с мёдом", Description = "Травяной, успокаивающий", Calories = 50, Image = "images/Tea.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("drinks")) },
    new Dish { Id = 17, Name = "Кофе латте", Description = "С молоком и пенкой", Calories = 120, Image = "images/Latte.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("drinks")) },
    new Dish { Id = 18, Name = "Морс", Description = "Из клюквы, свежий", Calories = 70, Image = "images/Mors.jpg", Category = _categories.Find(c => c.NormalizedName.Equals("drinks")) }
};
        }

        public async Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            int itemsPerPage = _config.GetValue<int>("ItemsPerPage");
            var query = _dishes.AsQueryable();
            if (!string.IsNullOrEmpty(categoryNormalizedName))
            {
                query = query.Where(d => d.Category != null && d.Category.NormalizedName == categoryNormalizedName);
            }
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

            var data = query.Skip((pageNo - 1) * itemsPerPage).Take(itemsPerPage).ToList();

            var listModel = new ListModel<Dish>
            {
                Items = data,
                CurrentPage = pageNo,
                TotalPages = totalPages
            };
            return ResponseData<ListModel<Dish>>.Success(listModel);
        }

        // Остальные методы (пока заглушки, реализуем позже если нужно)
        public Task<ResponseData<Dish>> GetProductByIdAsync(int id)
        {
            var dish = _dishes.FirstOrDefault(d => d.Id == id);
            return Task.FromResult(dish != null ? ResponseData<Dish>.Success(dish) : ResponseData<Dish>.Error("Not found"));
        }

        public Task<ResponseData<object>> UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            // Реализуй или верни заглушку
            return Task.FromResult(ResponseData<object>.Success(null));
        }

        public Task<ResponseData<object>> DeleteProductAsync(int id)
        {
            // Реализуй или заглушка
            return Task.FromResult(ResponseData<object>.Success(null));
        }

        public Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            // Заглушка
            return Task.FromResult(ResponseData<Dish>.Success(product));
        }
    }
}
