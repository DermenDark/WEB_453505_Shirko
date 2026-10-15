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
        }
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.Headers.TryGetValue(
                       "X-Requested-With",
                       out var requestedWith)
                   && string.Equals(
                       requestedWith.ToString(),
                       "XMLHttpRequest",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
