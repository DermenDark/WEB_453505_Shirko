using System.Net.Http.Headers;
using System.Text.Json;

using LRs.HelperClasses;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace LRs.Services.Authentication;

public sealed class KeycloakTokenAccessor : ITokenAccessor
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly KeycloakData _keycloakData;
    private readonly HttpClient _httpClient;

    public KeycloakTokenAccessor(
        IHttpContextAccessor contextAccessor,
        IOptions<KeycloakData> options,
        HttpClient httpClient)
    {
        _contextAccessor = contextAccessor;
        _keycloakData = options.Value;
        _httpClient = httpClient;
    }

    public async Task SetAuthorizationHeaderAsync(
        HttpClient httpClient,
        bool isClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        string token = isClient
            ? await GetClientTokenAsync()
            : await GetUserTokenAsync();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetUserTokenAsync()
    {
        var context = _contextAccessor.HttpContext
            ?? throw new AuthenticationFailureException(
                "HTTP-контекст отсутствует.");

        if (context.User.Identity?.IsAuthenticated != true)
        {
            throw new AuthenticationFailureException(
                "Пользователь не авторизован.");
        }

        // OpenID Connect сохраняет токены в cookie-аутентификации,
        // поскольку в Program.cs включён options.SaveTokens = true.
        var token = await context.GetTokenAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            "access_token");

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new AuthenticationFailureException(
                "Не удалось получить access_token пользователя.");
        }

        return token;
    }

    private async Task<string> GetClientTokenAsync()
    {
        var tokenEndpoint =
            $"{_keycloakData.Host.TrimEnd('/')}" +
            $"/realms/{_keycloakData.Realm}" +
            "/protocol/openid-connect/token";

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = _keycloakData.ClientId,
                ["grant_type"] = "client_credentials",
                ["client_secret"] = _keycloakData.ClientSecret
            });

        using var response = await _httpClient.PostAsync(
            tokenEndpoint,
            content);

        var responseBody =
            await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Keycloak не выдал токен клиента. " +
                $"Код: {(int)response.StatusCode}. " +
                $"Ответ: {responseBody}");
        }

        using var json = JsonDocument.Parse(responseBody);

        if (!json.RootElement.TryGetProperty(
                "access_token",
                out var tokenElement))
        {
            throw new InvalidOperationException(
                "В ответе Keycloak отсутствует access_token.");
        }

        var token = tokenElement.GetString();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Keycloak вернул пустой access_token.");
        }

        return token;
    }
}