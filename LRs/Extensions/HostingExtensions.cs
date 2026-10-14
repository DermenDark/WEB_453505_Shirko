using LRs.HelperClasses;
using LRs.Services.BikeCategoryService;
using LRs.Services.BikeProductService;

namespace LRs.Extensions
{
    public static class HostingExtensions
    {
        public static void RegisterCustomServices(
        this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IBikeCategoryService, MemoryCategoryService>();
            builder.Services.AddScoped<IBikeProductService, MemoryProductService>();
            builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));
        }
    }
}
