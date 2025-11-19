using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using WEB_353504_Bogdanovich.UI.Extensions;
using WEB_353504_Bogdanovich.UI.HelperClasses;
using WEB_353504_Bogdanovich.UI.Services.Authentication;
using WEB_353504_Bogdanovich.UI.Services.FileService;

var builder = WebApplication.CreateBuilder(args);

var keycloakData = builder.Configuration.GetSection("Keycloak").Get<KeycloakData>();
builder.Services
    .Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.RegisterCustomServices();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenAccessor, KeycloakTokenAccessor>();
builder.Services.AddHttpClient("KeycloakTokenClient");

builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;
    client.BaseAddress = new Uri($"{apiUri}categories/");
});

builder.Services.AddHttpClient<IProductService, ApiProductService>(client =>
{
    var apiUri = builder.Configuration.GetSection("UriData:ApiUri").Value;
    client.BaseAddress = new Uri(apiUri);
});

builder.Services.AddScoped<IFileService, LocalFileService>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "keycloak";
    })
.AddCookie()
.AddOpenIdConnect("keycloak", options =>
{
    options.Authority = $"{keycloakData.Host}/realms/{keycloakData.Realm}";
    options.ClientId = keycloakData.ClientId;
    options.ClientSecret = keycloakData.ClientSecret;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.Scope.Add("openid");
    options.SaveTokens = true;
    options.RequireHttpsMetadata = false;
    options.MetadataAddress = $"{keycloakData.Host}/realms/{keycloakData.Realm}/.well-known/openid-configuration";
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
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

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages()
    .RequireAuthorization("admin");

app.Run();