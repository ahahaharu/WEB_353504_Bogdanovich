using WEB_353504_Bogdanovich.UI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.RegisterCustomServices();

builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;
    client.BaseAddress = new Uri($"{apiUri}categories/");
});

builder.Services.AddHttpClient<IProductService, ApiProductService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;
    client.BaseAddress = new Uri($"{apiUri}Dish/");
});

builder.Services.AddRazorPages();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();

builder.RegisterCustomServices();
