using MediatR;

namespace WEB_453505_Shirko.API.UseCases
{
    public sealed record SaveImage(IFormFile file) : IRequest<string>;
    public class SaveImageHandler(
    IWebHostEnvironment env,
    IHttpContextAccessor httpContextAccessor)
    :IRequestHandler<SaveImage, string>
    {
        public async Task<string> Handle(SaveImage request , CancellationToken cancellationToken)
        {
            var file = request.file;

            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл пустой.");

            var extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            var allowedExtensions = new HashSet<string>
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException(
                    "Допустимые форматы: JPG, JPEG, PNG, WEBP.");

            var httpRequest = httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException(
                    "HTTP-контекст недоступен.");

            var webRootPath = env.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(
                    env.ContentRootPath, "wwwroot");

            var imagesPath = Path.Combine(webRootPath, "Images");
            Directory.CreateDirectory(imagesPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(imagesPath, fileName);

            await using (var stream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var imageUrl =
                $"{httpRequest.Scheme}://{httpRequest.Host}" +
                $"{httpRequest.PathBase}/Images/{fileName}";

            return imageUrl;
        }
    }
}
