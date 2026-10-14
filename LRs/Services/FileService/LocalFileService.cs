using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace LRs.Services.FileService;

public sealed class LocalFileService(
    IWebHostEnvironment environment) : IFileService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (file.Length <= 0)
        {
            throw new InvalidOperationException(
                "Загруженный файл пуст.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Допускаются только JPG, PNG, GIF и WebP.");
        }

        var webRoot = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(
                environment.ContentRootPath,
                "wwwroot");
        }

        var imagesDirectory = Path.Combine(
            webRoot,
            "Images");

        Directory.CreateDirectory(imagesDirectory);

        // Генерируем случайное уникальное имя файла.
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var filePath = Path.Combine(
            imagesDirectory,
            fileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await file.CopyToAsync(stream);

        // Возвращаем URL, а не физический путь на диске.
        return $"/Images/{fileName}";
    }
}