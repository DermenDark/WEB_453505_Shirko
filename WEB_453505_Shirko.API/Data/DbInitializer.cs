using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.API.Data;

public static class DbInitializer
{
    public static async Task SeedData(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        var apiUrl = app.Configuration["ApiUrl"];


        if (await context.Bikes.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Name = "Шоссейные велосипеды",
                NormalizedName = "road"
            },

            new Category
            {
                Name = "Грэвел велосипеды",
                NormalizedName = "gravel"
            },

            new Category
            {
                Name = "Горные велосипеды",
                NormalizedName = "mountain"
            },

            new Category
            {
                Name = "Фиксы",
                NormalizedName = "fixie"
            },

            new Category
            {
                Name = "Гибридные велосипеды",
                NormalizedName = "hybrid"
            }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();

        var bikes = new List<Bike>
        {
            new Bike
            {
                Name = "Шоссейник Aero",
                Description = "Лёгкая алюминиевая рама и узкие покрышки для быстрой езды по асфальту",
                Price = 1600,
                Image = $"{apiUrl}/Images/road.jpg",
                MimeType = "image/jpeg",
                CategoryId = categories
                    .First(c => c.NormalizedName == "road")
                    .Id
            },

            new Bike
            {
                Name = "Грэвел Adventure",
                Description = "Универсал: асфальт, грунт, лёгкое бездорожье",
                Price = 4400,
                Image = $"{apiUrl}/Images/gravel.jpg",
                MimeType = "image/jpeg",
                CategoryId = categories
                    .First(c => c.NormalizedName == "gravel")
                    .Id
            },

            new Bike
            {
                Name = "Горный Trail Pro",
                Description = "Амортизационная вилка, широкие покрышки",
                Price = 1000,
                Image = $"{apiUrl}/Images/mountain.jpg",
                MimeType = "image/jpeg",
                CategoryId = categories
                    .First(c => c.NormalizedName == "mountain")
                    .Id
            },

            new Bike
            {
                Name = "Фикс SingleSpeed",
                Description = "Одна передача, минимализм, стиль города",
                Price = 1300,
                Image = $"{apiUrl}/Images/fixie.jpg",
                MimeType = "image/jpeg",
                CategoryId = categories
                    .First(c => c.NormalizedName == "fixie")
                    .Id
            },

            new Bike
            {
                Name = "Гибрид City",
                Description = "Комфорт для города и лёгких прогулок",
                Price = 700,
                Image = $"{apiUrl}/Images/hybrid.jpg",
                MimeType = "image/jpeg",
                CategoryId = categories
                    .First(c => c.NormalizedName == "hybrid")
                    .Id
            }
        };

        // Добавляем велосипеды в БД
        await context.Bikes.AddRangeAsync(bikes);
        await context.SaveChangesAsync();
    }
}