
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using WEB_453505_Shirko.API.Abstractions;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.API.UseCases;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace WEB_453505_Shirko.API.Features;

public static class GetAllProducts
{
    /// <summary>
    /// Конечные точки получения велосипедов.
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                    "api/Bike/{category:alpha?}",
                    Handler)
                .WithName("GetAllBikes")
                .WithTags("Bikes")
                .AllowAnonymous();

            app.MapGet(
                    "api/Bike/{id:int}",
                    GetByIdHandler)
                .WithName("GetBikeById")
                .WithTags("Bikes")
                .AllowAnonymous();
        }
    }

    /// <summary>
    /// Получение списка велосипедов с кэшированием.
    /// </summary>
    public static async Task<Ok<ResponseData<ListModel<Bike>>>> Handler(
        string? category,
        IMediator mediator,
        HybridCache cache,
        CancellationToken cancellationToken,
        int pageNo = 1,
        int pageSize = 3)
    {
        const int maxPageSize = 20;

        category = string.IsNullOrWhiteSpace(category)
            ? null
            : category.Trim().ToLowerInvariant();

        // Нормализуем параметры до формирования ключа.
        var requestedPageNo = Math.Max(1, pageNo);
        var requestedPageSize = Math.Clamp(pageSize, 1, maxPageSize);

        var categoryKey = category ?? "all";

        var cacheKey =
            $"bikes_{categoryKey}_page_{requestedPageNo}_size_{requestedPageSize}";

        var response = await cache.GetOrCreateAsync<
            ResponseData<ListModel<Bike>>>(
            cacheKey,

            async token => await mediator.Send(
                new GetListOfProducts(
                    category,
                    requestedPageNo,
                    requestedPageSize),
                token),

            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(1),
                LocalCacheExpiration = TimeSpan.FromSeconds(30)
            },

            cancellationToken: cancellationToken);

        return TypedResults.Ok(response);
    }

    /// <summary>
    /// Получение одного велосипеда по ID.
    /// </summary>
    public static async Task<IResult> GetByIdHandler(
        int id,
        AppDbContext db)
    {
        var bike = await db.Bikes
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);

        return bike is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(bike);
    }
}
