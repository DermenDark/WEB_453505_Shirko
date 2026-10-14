using Microsoft.AspNetCore.Http;

namespace LRs.Services.FileService;

public interface IFileService
{
    /// <summary>
    /// Сохраняет изображение в wwwroot/Images.
    /// Возвращает URL сохранённого изображения.
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file);
}