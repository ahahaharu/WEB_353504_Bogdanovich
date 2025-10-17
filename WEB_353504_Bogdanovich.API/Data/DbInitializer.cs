using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.Domain.Entities;

namespace WEB_353504_Bogdanovich.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();

            // Категории (если нет)
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Стартеры", NormalizedName = "starters" },
                    new Category { Name = "Салаты", NormalizedName = "salads" },
                    new Category { Name = "Супы", NormalizedName = "soups" },
                    new Category { Name = "Основные блюда", NormalizedName = "main-dishes" },
                    new Category { Name = "Напитки", NormalizedName = "drinks" },
                    new Category { Name = "Десерты", NormalizedName = "desserts" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Блюда (если нет)
            if (!await context.Dishes.AnyAsync())
            {
                var categories = await context.Categories.ToListAsync();
                var dishes = new List<Dish>
                {
                    new Dish { Name = "Суп-харчо", Description = "Очень острый, невкусный", Calories = 200, Image = "Images/Sup.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Борщ", Description = "Много сала, без сметаны", Calories = 330, Image = "Images/Borsch.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Куриный бульон", Description = "С фрикадельками, по бабушкиному рецепту", Calories = 450, Image = "Images/Kuriniy_bulon.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Шурпа", Description = "Дичь какая-то", Calories = 300, Image = "Images/Shurpa.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Тыквенный суп", Description = "Со сливками", Calories = 340, Image = "Images/Tikvenniy_sup.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Солянка", Description = "Сборная", Calories = 400, Image = "Images/Solyanka.jpg", CategoryId = categories.Find(c => c.NormalizedName == "soups").Id },
                    new Dish { Name = "Плов", Description = "С бараниной и специями", Calories = 600, Image = "Images/Plov.jpg", CategoryId = categories.Find(c => c.NormalizedName == "main-dishes").Id },
                    new Dish { Name = "Котлета по-киевски", Description = "С маслом внутри", Calories = 550, Image = "Images/Kotleta_kievskaya.jpg", CategoryId = categories.Find(c => c.NormalizedName == "main-dishes").Id },
                    new Dish { Name = "Жареная картошка", Description = "С луком и грибами", Calories = 480, Image = "Images/Zhar_kartoshka.jpg", CategoryId = categories.Find(c => c.NormalizedName == "main-dishes").Id },
                    new Dish { Name = "Чизкейк", Description = "Нью-Йоркский стиль", Calories = 400, Image = "Images/Cheesecake.jpg", CategoryId = categories.Find(c => c.NormalizedName == "desserts").Id },
                    new Dish { Name = "Тирамису", Description = "С кофе и маскарпоне", Calories = 350, Image = "Images/Tiramisu.jpg", CategoryId = categories.Find(c => c.NormalizedName == "desserts").Id },
                    new Dish { Name = "Медовик", Description = "С кремом из сгущёнки", Calories = 450, Image = "Images/Medovik.jpg", CategoryId = categories.Find(c => c.NormalizedName == "desserts").Id }
                };
                await context.Dishes.AddRangeAsync(dishes);
                await context.SaveChangesAsync();
            }
        }
    }
}
