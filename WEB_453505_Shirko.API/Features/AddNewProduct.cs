using FluentValidation;
using MediatR;
using System.Text.Json;
using WEB_453505_Shirko.API.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Abstractions;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.API.Features;

public static class AddNewProduct
{
    // DTO с данными нового велосипеда
    public sealed record RequestDTO(
        string Name,
        string Description,
        float Price,
        int CategoryId,
        string? Image,
        string? MimeType);

    /// <summary>
    /// Валидатор DTO
    /// </summary>
    public class RequestValidator : AbstractValidator<RequestDTO>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(r => r.Description)
                .NotEmpty();

            RuleFor(r => r.Price)
                .GreaterThan(0);

            RuleFor(r => r.CategoryId)
                .GreaterThan(0);
        }
    }

    /// <summary>
    /// Endpoint вертикального среза
    /// </summary>
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/Bike", Handler)
                .WithName("CreateBike")
                .WithTags("Bikes")
                .DisableAntiforgery().RequireAuthorization("admin");
            app.MapPut("api/Bike/{id:int}", UpdateHandler)
                .WithName("UpdateBike")
                .WithTags("Bikes")
                .DisableAntiforgery().RequireAuthorization("admin");
            app.MapDelete("api/Bike/{id:int}", DeleteHandler)
                .WithName("DeleteBike")
                .WithTags("Bikes").RequireAuthorization("admin");
        }

        /// <summary>
        /// Обработчик добавления нового велосипеда
        /// </summary>

        public static async Task<IResult> Handler(
            [FromForm] string bike,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            IValidator<RequestDTO> validator)
        {
            // Получаем данные велосипеда из JSON-строки формы
            RequestDTO? request;

            try
            {
                request = JsonSerializer.Deserialize<RequestDTO>(
                    bike,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException)
            {
                return TypedResults.BadRequest("Некорректные данные велосипеда.");
            }

            if (request is null)
                return TypedResults.BadRequest("Данные велосипеда не переданы.");

            // Проверяем данные через FluentValidation
            var validationResult = await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return TypedResults.BadRequest(
                    validationResult.Errors.Select(e => e.ErrorMessage));
            }

            // Проверяем существование категории
            var categoryExists = await db.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
                return TypedResults.BadRequest("Категория не найдена.");

            // Создаём велосипед
            var newBike = new Bike
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                Image = request.Image,
                MimeType = request.MimeType
            };

            // Если изображение передано, сохраняем его через MediatR
            if (file is not null)
            {
                if (file.Length == 0)
                    return TypedResults.BadRequest("Файл изображения пуст.");

                newBike.Image = await mediator.Send(new SaveImage(file));
                newBike.MimeType = file.ContentType;
            }

            // Сохраняем велосипед в базе данных
            db.Bikes.Add(newBike);
            await db.SaveChangesAsync();

            return TypedResults.Created(
                $"/api/Bike/{newBike.Id}",
                newBike);
        }

        public static async Task<IResult> UpdateHandler(
            int id,
            [FromForm] string bike,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            IValidator<RequestDTO> validator)
        {
            // 1. Преобразуем JSON из поля формы в DTO
            RequestDTO? request;

            try
            {
                request = JsonSerializer.Deserialize<RequestDTO>(
                    bike,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException)
            {
                return TypedResults.BadRequest(
                    "Некорректные данные велосипеда.");
            }

            if (request is null)
            {
                return TypedResults.BadRequest(
                    "Данные велосипеда не переданы.");
            }

            // 2. Проверяем данные
            var validationResult =
                await validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return TypedResults.BadRequest(
                    validationResult.Errors.Select(e => e.ErrorMessage));
            }

            // 3. Находим существующий велосипед в основной БД
            var existingBike = await db.Bikes
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBike is null)
            {
                return TypedResults.NotFound(
                    $"Велосипед с ID {id} не найден.");
            }

            // 4. Проверяем, существует ли указанная категория
            var categoryExists = await db.Categories
                .AnyAsync(c => c.Id == request.CategoryId);

            if (!categoryExists)
            {
                return TypedResults.BadRequest(
                    "Указанная категория не существует.");
            }

            // 5. Обновляем обычные поля велосипеда
            existingBike.Name = request.Name;
            existingBike.Description = request.Description;
            existingBike.Price = request.Price;
            existingBike.CategoryId = request.CategoryId;

            // 6. Меняем изображение только при загрузке нового файла
            if (file is not null)
            {
                if (file.Length == 0)
                {
                    return TypedResults.BadRequest(
                        "Файл изображения пуст.");
                }

                existingBike.Image =
                    await mediator.Send(new SaveImage(file));

                existingBike.MimeType = file.ContentType;
            }

            // 7. Сохраняем изменения в PostgreSQL
            await db.SaveChangesAsync();

            return TypedResults.Ok(existingBike);
        }
        public static async Task<IResult> DeleteHandler(
        int id,
        AppDbContext db)
        {
            var bike = await db.Bikes.FindAsync(id);

            if (bike is null)
                return TypedResults.NotFound();

            db.Bikes.Remove(bike);
            await db.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}