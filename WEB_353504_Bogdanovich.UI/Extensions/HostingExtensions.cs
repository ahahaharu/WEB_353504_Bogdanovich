using Microsoft.AspNetCore.Builder;
using WEB_353504_Bogdanovich.UI.HelperClasses;
using WEB_353504_Bogdanovich.UI.Services.CategoryService;
using WEB_353504_Bogdanovich.UI.Services.ProductService;

namespace WEB_353504_Bogdanovich.UI.Extensions
{
    public static class HostingExtensions
    {
        public static void RegisterCustomServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ICategoryService, MemoryCategoryService>();
            builder.Services.AddScoped<IProductService, MemoryProductService>();
            var keycloakData = builder.Configuration.GetSection("Keycloak").Get<KeycloakData>();
            builder.Services
                .Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));
        }
    }
}
