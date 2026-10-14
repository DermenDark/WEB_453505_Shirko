using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Abstractions;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace WEB_453505_Shirko.API.Features;

public static class GetAllCategories
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "api/categories",
                Handler)
                .WithName("GetAllCategories")
                .WithTags("Categories").AllowAnonymous();
        }

        public static async Task<
            Ok<ResponseData<List<Category>>>> Handler(
            AppDbContext db)
        {
            var categories = await db.Categories
                .AsNoTracking()
                .ToListAsync();

            return TypedResults.Ok(
                new ResponseData<List<Category>>
                {
                    Data = categories,
                    Successfull = true
                });
        }
    }
}