using System.Text.Json;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.ApiService;

public class ApiCategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiCategoryService> _logger;

    private readonly JsonSerializerOptions _serializerOptions =
        new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    public ApiCategoryService(
        HttpClient httpClient,
        ILogger<ApiCategoryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ResponseData<List<Category>>>
        GetCategoryListAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("");

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content
                    .ReadFromJsonAsync<
                        ResponseData<List<Category>>>(
                        _serializerOptions);

                return data
                    ?? ResponseData<List<Category>>.Error(
                        "Пустой ответ от сервера.");
            }

            _logger.LogError(
                "Ошибка получения категорий. StatusCode: {StatusCode}",
                response.StatusCode);

            return ResponseData<List<Category>>.Error(
                $"Ошибка получения категорий: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Ошибка обращения к API категорий: {Message}",
                ex.Message);

            return ResponseData<List<Category>>.Error(
                $"Ошибка: {ex.Message}");
        }
    }
}