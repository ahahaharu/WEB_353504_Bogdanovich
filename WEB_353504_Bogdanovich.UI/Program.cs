using WEB_353504_Bogdanovich.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.RegisterCustomServices();

builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;
    // Для категорий оставляем, так как API-эндпоинт категорий не имеет префикса "categories/" в методах
    client.BaseAddress = new Uri($"{apiUri}categories/");
});

builder.Services.AddHttpClient<IProductService, ApiProductService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;

    // !!! ИСПРАВЛЕНИЕ: BaseAddress должен указывать только на корень API (например, https://localhost:7002/api/)
    // !!! Сервис сам добавит "Dish/{id}"
    client.BaseAddress = new Uri(apiUri);
});

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();