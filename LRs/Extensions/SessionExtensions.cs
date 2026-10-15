using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace LRs.Extensions;

public static class SessionExtensions
{
    /// <summary>
    /// Сохраняет объект в сессии в формате JSON.
    /// </summary>
    public static void Set<T>(
        this ISession session,
        string key,
        T value)
    {
        var json = JsonSerializer.Serialize(value);

        session.SetString(key, json);
    }

    /// <summary>
    /// Получает объект из сессии.
    /// Если значение отсутствует, возвращает default.
    /// </summary>
    public static T? Get<T>(
        this ISession session,
        string key)
    {
        var json = session.GetString(key);

        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(json);
    }
}