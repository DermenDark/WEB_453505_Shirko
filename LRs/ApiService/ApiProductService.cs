using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LRs.Services.Authentication;
using Microsoft.AspNetCore.Http;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.ApiService;

public class ApiProductService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly int _pageSize;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ApiProductService> _logger;
    private readonly ITokenAccessor _tokenAccessor;

    public ApiProductService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ApiProductService> logger,
        ITokenAccessor tokenAccessor)
    {
        _httpClient = httpClient;
        _logger = logger;
        _tokenAccessor = tokenAccessor;

        _pageSize = configuration.GetValue<int>("ItemsPerPage");

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ResponseData<Bike>> GetProductByIdAsync(int id)
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            using var response = await _httpClient.GetAsync(GetBikeUri(id));

            if (response.IsSuccessStatusCode)
                return await ReadBikeResponseAsync(response);

            var error = await response.Content.ReadAsStringAsync();
            return ResponseData<Bike>.Error(
                $"Велосипед не получен. Код: {response.StatusCode}. Ответ: {error}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения велосипеда {Id}", id);
            return ResponseData<Bike>.Error(ex.Message);
        }
    }

    public async Task<ResponseData<List<Category>>> GetCategoriesAsync()
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            var baseAddress = _httpClient.BaseAddress
                ?? throw new InvalidOperationException(
                    "Не задан BaseAddress для API.");

            var categoriesUri = new UriBuilder(baseAddress)
            {
                Path = "/api/categories",
                Query = ""
            }.Uri;

            using var response = await _httpClient.GetAsync(categoriesUri);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ResponseData<List<Category>>.Error(
                    $"Категории не получены. Код: {response.StatusCode}. Ответ: {error}");
            }

            return await response.Content
                .ReadFromJsonAsync<ResponseData<List<Category>>>(
                    _serializerOptions)
                ?? ResponseData<List<Category>>.Error(
                    "API вернул пустой ответ.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка получения категорий");
            return ResponseData<List<Category>>.Error(ex.Message);
        }
    }

    public async Task<ResponseData<ListModel<Bike>>> GetProductListAsync(
        string? categoryNormalizedName,
        int pageNo = 1)
    {
        try
        {
            var baseAddress = _httpClient.BaseAddress
                ?? throw new InvalidOperationException(
                    "Не задан BaseAddress для API.");

            var urlString = new StringBuilder(baseAddress.AbsoluteUri);

            // Добавляем категорию в маршрут.
            if (!string.IsNullOrEmpty(categoryNormalizedName))
                urlString.Append($"{categoryNormalizedName}/");

            // Номер страницы.
            if (pageNo > 1)
                urlString.Append($"?pageNo={pageNo}");

            // Размер страницы.
            var separator = urlString.ToString().Contains("?") ? "&" : "?";
            urlString.Append($"{separator}pageSize={_pageSize}");

            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            using var response = await _httpClient.GetAsync(
                new Uri(urlString.ToString()));

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    return await response.Content
                        .ReadFromJsonAsync<ResponseData<ListModel<Bike>>>(
                            _serializerOptions)
                        ?? ResponseData<ListModel<Bike>>.Error(
                            "Пустой ответ от сервера.");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Ошибка десериализации списка велосипедов");
                    return ResponseData<ListModel<Bike>>.Error(
                        $"Ошибка десериализации: {ex.Message}");
                }
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Данные не получены от сервера. Код: {StatusCode}. Ответ: {Error}",
                response.StatusCode, error);

            return ResponseData<ListModel<Bike>>.Error(
                $"Данные не получены от сервера. Код: {response.StatusCode}. Ответ: {error}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обращения к API при получении списка велосипедов");
            return ResponseData<ListModel<Bike>>.Error(
                $"Ошибка обращения к API: {ex.Message}");
        }
    }

    /// <summary>
    /// Создание велосипеда.
    /// </summary>
    public async Task<ResponseData<Bike>> CreateProductAsync(
        Bike product,
        IFormFile? formFile)
    {
        if (formFile == null)
            product.Image = "Images/noimage.jpg";
        else
            product.MimeType = formFile.ContentType;

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post, GetBikeUri());

            request.Content = CreateMultipartContent(product, formFile);

            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            using var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return await ReadBikeResponseAsync(response);

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "Велосипед не создан. Код: {StatusCode}. Ответ: {Error}",
                response.StatusCode, error);

            return ResponseData<Bike>.Error(
                $"Велосипед не создан. Код: {response.StatusCode}. Ответ: {error}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании велосипеда");
            return ResponseData<Bike>.Error(
                $"Ошибка при создании велосипеда: {ex.Message}");
        }
    }

    /// <summary>
    /// Обновление велосипеда.
    /// </summary>
    public async Task UpdateProductAsync(
        int id,
        Bike product,
        IFormFile? formFile)
    {
        try
        {
            if (formFile != null)
                product.MimeType = formFile.ContentType;

            using var request = new HttpRequestMessage(
                HttpMethod.Put, GetBikeUri(id));

            request.Content = CreateMultipartContent(product, formFile);

            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Не удалось обновить велосипед. Код: {response.StatusCode}. Ответ: {error}",
                    null,
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении велосипеда с ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Удаление велосипеда.
    /// </summary>
    public async Task DeleteProductAsync(int id)
    {
        try
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(
                _httpClient, isClient: false);

            using var response = await _httpClient.DeleteAsync(GetBikeUri(id));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"Не удалось удалить велосипед. Код: {response.StatusCode}. Ответ: {error}",
                    null,
                    response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении велосипеда с ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Формирует multipart/form-data с данными велосипеда и изображением.
    /// </summary>
    private MultipartFormDataContent CreateMultipartContent(
        Bike product,
        IFormFile? formFile)
    {
        var content = new MultipartFormDataContent();

        if (formFile != null && formFile.Length > 0)
        {
            var streamContent = new StreamContent(formFile.OpenReadStream());
            content.Add(streamContent, "file", Path.GetFileName(formFile.FileName));
        }

        var json = JsonSerializer.Serialize(product, _serializerOptions);
        content.Add(new StringContent(json, Encoding.UTF8), "bike");

        return content;
    }

    /// <summary>
    /// Формирует URL конечной точки API для велосипеда.
    /// </summary>
    private Uri GetBikeUri(int? id = null)
    {
        var baseAddress = _httpClient.BaseAddress
            ?? throw new InvalidOperationException(
                "Не задан BaseAddress для API.");

        var url = baseAddress.AbsoluteUri.TrimEnd('/');

        if (id.HasValue)
            url += $"/{id.Value}";

        return new Uri(url);
    }

    /// <summary>
    /// Читает ответ API как Bike или ResponseData&lt;Bike&gt;.
    /// </summary>
    private async Task<ResponseData<Bike>> ReadBikeResponseAsync(
        HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(json))
            return ResponseData<Bike>.Error("API вернул пустой ответ.");

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Object &&
            (root.TryGetProperty("data", out _) ||
             root.TryGetProperty("Data", out _)))
        {
            return JsonSerializer.Deserialize<ResponseData<Bike>>(
                       json, _serializerOptions)
                   ?? ResponseData<Bike>.Error(
                       "Не удалось прочитать ответ API.");
        }

        var bike = JsonSerializer.Deserialize<Bike>(json, _serializerOptions);

        if (bike == null)
            return ResponseData<Bike>.Error(
                "Не удалось прочитать созданный велосипед.");

        return new ResponseData<Bike>
        {
            Data = bike,
            Successfull = true
        };
    }
}
