using System.Net;
using System.Text;
using System.Text.Json;

using LRs.HelperClasses;
using LRs.Models;
using LRs.Services.Authentication;
using LRs.Services.FileService;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace LRs.Controllers;

[AllowAnonymous]
public sealed class AccountController : Controller
{

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterUserViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        [FromForm] RegisterUserViewModel user,
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ITokenAccessor tokenAccessor,
        [FromServices] IOptions<KeycloakData> options,
        [FromServices] IFileService fileService,
        [FromServices] ILogger<AccountController> logger)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        // Ограничиваем размер аватара до 5 МБ.
        const long maxAvatarSize = 5 * 1024 * 1024;

        if (user.Avatar is { Length: > 0 })
        {
            if (user.Avatar.Length > maxAvatarSize)
            {
                ModelState.AddModelError(
                    nameof(user.Avatar),
                    "Размер аватара не должен превышать 5 МБ.");

                return View(user);
            }

            var extension = Path.GetExtension(user.Avatar.FileName);

            var allowedExtensions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(user.Avatar),
                    "Допускаются изображения JPG, JPEG, PNG, GIF и WebP.");

                return View(user);
            }
        }

        var httpClient = httpClientFactory.CreateClient();

        // 1. Получаем клиентский access_token для Admin API Keycloak.
        try
        {
            await tokenAccessor.SetAuthorizationHeaderAsync(
                httpClient,
                isClient: true);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Не удалось получить клиентский токен Keycloak.");

            ModelState.AddModelError(
                string.Empty,
                "Не удалось подключиться к серверу аутентификации.");

            return View(user);
        }

        // 2. Определяем URL аватара.
        var avatarUrl = "/Images/default-profile-picture.png";

        try
        {
            if (user.Avatar is { Length: > 0 })
            {
                avatarUrl = await fileService.SaveFileAsync(user.Avatar);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка сохранения аватара.");

            ModelState.AddModelError(
                nameof(user.Avatar),
                "Не удалось сохранить изображение.");

            return View(user);
        }

        // 3. Подготавливаем данные нового пользователя.
        var newUser = new CreateUserModel
        {
            Username = user.Email.Trim(),
            Email = user.Email.Trim(),
            Enabled = true,
            EmailVerified = true,

            // Keycloak UserRepresentation ожидает массивы
            // значений в атрибутах пользователя.
            Attributes = new Dictionary<string, string[]>
            {
                ["avatar"] = new[] { avatarUrl }
            },

            Credentials = new List<UserCredentials>
        {
            new()
            {
                Type = "password",
                Temporary = false,
                Value = user.Password
            }
        }
        };

        var realm = Uri.EscapeDataString(options.Value.Realm);

        var requestUri =
            $"{options.Value.Host.TrimEnd('/')}" +
            $"/admin/realms/{realm}/users";

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var userData = JsonSerializer.Serialize(
            newUser,
            serializerOptions);

        using var content = new StringContent(
            userData,
            Encoding.UTF8,
            "application/json");

        // 4. Регистрируем пользователя в Keycloak.
        try
        {
            using var response = await httpClient.PostAsync(
                requestUri,
                content);

            if (response.IsSuccessStatusCode)
            {
                TempData["RegisterSuccess"] =
                    "Регистрация прошла успешно. Теперь вы можете войти.";

                return RedirectToAction("Index", "Home");
            }

            var errorBody = await response.Content.ReadAsStringAsync();

            logger.LogWarning(
                "Keycloak не зарегистрировал пользователя. " +
                "Статус: {StatusCode}. Ответ: {Response}",
                (int)response.StatusCode,
                errorBody);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    nameof(user.Email),
                    "Пользователь с таким email уже существует.");
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "У приложения нет прав на регистрацию пользователей в Keycloak. " +
                    "Проверьте Service account roles.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Keycloak не зарегистрировал пользователя. " +
                    $"Код ответа: {(int)response.StatusCode}.");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Ошибка запроса к Admin API Keycloak.");

            ModelState.AddModelError(
                string.Empty,
                "Не удалось связаться с сервером регистрации.");
        }

        return View(user);
    }
    private sealed class CreateUserModel
    {
        public Dictionary<string, string[]> Attributes { get; set; } = new();

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool Enabled { get; set; } = true;

        public bool EmailVerified { get; set; } = true;

        public List<UserCredentials> Credentials { get; set; } = new();
    }

    private sealed class UserCredentials
    {
        public string Type { get; set; } = "password";

        public bool Temporary { get; set; } = false;

        public string Value { get; set; } = string.Empty;
    }
}
