using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Abstractions;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace WEB_453505_Shirko.API.Features;

public static class GetAllProducts
{
    /// <summary>
    /// Конечная точка получения списка велосипедов
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "api/Bike/{category:alpha?}",
                Handler)
                .WithName("GetAllBikes")
                .WithTags("Bikes").AllowAnonymous();
            app.MapGet("api/Bike/{id:int}", GetByIdHandler)
                .WithName("GetBikeById")
                .WithTags("Bikes").AllowAnonymous();
        }

        /// <summary>
        /// Получение списка велосипедов
        /// </summary>
        public static async Task<
            Ok<ResponseData<ListModel<Bike>>>> Handler(
            string? category,
            int pageNo = 1,
            int pageSize = 3,
            AppDbContext db = null!)
        {
            // Максимальный размер страницы
            const int maxPageSize = 20;

            if (pageSize < 1)
                pageSize = 1;

            if (pageSize > maxPageSize)
                pageSize = maxPageSize;

            if (pageNo < 1)
                pageNo = 1;

            // Начальный запрос
            var query = db.Bikes
                .AsNoTracking()
                .AsQueryable();

            // Фильтрация по категории
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b =>
                    b.Category != null &&
                    b.Category.NormalizedName == category);
            }

            // Количество объектов
            var totalItems = await query.CountAsync();

            // Формирование результата
            var result = new ListModel<Bike>();

            result.TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            if (result.TotalPages == 0)
                result.TotalPages = 1;

            if (pageNo > result.TotalPages)
                pageNo = result.TotalPages;

            result.CurrentPage = pageNo;

            result.Items = await query
                .Skip((pageNo - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return TypedResults.Ok(
                new ResponseData<ListModel<Bike>>
                {
                    Data = result,
                    Successfull = true
                });
        }
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
}