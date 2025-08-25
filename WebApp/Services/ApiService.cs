using System.Text;
using System.Text.Json;

namespace WebApp.Services;

public class ApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _apiUrl;

    public ApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _apiUrl = config["API:url"] ?? throw new ArgumentNullException(nameof(_apiUrl));
    }

    /// <summary>
    /// Универсальный метод запроса к API
    /// </summary>
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiUrl}{endpoint}");

        // Добавляем куку из текущего запроса
        var cookie = _httpContextAccessor.HttpContext?.Request.Headers["Cookie"].ToString();
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.Add("Cookie", cookie);

        var response = await client.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"API StatusCode: {response.StatusCode}. " +
                $"Ответ сервера: {responseContent}"
            );

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? payload)
    {
        var client = _httpClientFactory.CreateClient();
        HttpContent? content = null;

        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await client.PostAsync($"{_apiUrl}{endpoint}", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            // Включаем в текст исключения тело ответа для отладки
            throw new HttpRequestException(
                $"API StatusCode: {response.StatusCode}. " + 
                $"Ответ сервера: {responseContent}"
            );
        }

        return string.IsNullOrWhiteSpace(responseContent)
            ? default
            : JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }



}

