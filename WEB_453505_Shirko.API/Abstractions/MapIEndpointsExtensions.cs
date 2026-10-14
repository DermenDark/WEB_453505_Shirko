using WEB_453505_Shirko.API.Abstractions;

namespace WEB_453505_Shirko.API.Extensions;

public static class MapIEndpointsExtensions
{
    /// <summary>
    /// Регистрация конечных точек,
    /// описанных с помощью IEndpoint
    /// </summary>
    public static void MapIEndpoints(this WebApplication app)
    {
        var endpoints = typeof(Program).Assembly
            .GetTypes()
            .Where(t =>
                typeof(IEndpoint).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .Select(type =>
                Activator.CreateInstance(type) as IEndpoint)
            .Where(endpoint => endpoint != null)
            .ToArray();

        foreach (var endpoint in endpoints)
        {
            endpoint!.MapEndpoint(app);
        }
    }
}