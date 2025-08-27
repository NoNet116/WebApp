using System.Net;
using System.Text;
using System.Text.Json;

namespace WebApp.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _apiUrl;
    private readonly CookieContainer _cookieContainer;

    public ApiService(HttpClient httpClient,
                     IHttpContextAccessor httpContextAccessor,
                     IConfiguration config)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _apiUrl = config["API:url"] ?? throw new ArgumentNullException(nameof(_apiUrl));
        _cookieContainer = new CookieContainer();

        // Настраиваем HttpClient
        _httpClient.BaseAddress = new Uri(_apiUrl);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        await AddCookiesToRequest();
        var response = await _httpClient.GetAsync(endpoint);
        await SaveCookiesFromResponse(response);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? payload)
    {
        await AddCookiesToRequest();

        HttpContent? content = null;
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.PostAsync(endpoint, content);
        await SaveCookiesFromResponse(response);
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? payload)
    {
        await AddCookiesToRequest();

        HttpContent? content = null;
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.PutAsync(endpoint, content);
        await SaveCookiesFromResponse(response);
        return await HandleResponse<T>(response);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        await AddCookiesToRequest();
        var response = await _httpClient.DeleteAsync(endpoint);
        await SaveCookiesFromResponse(response);
        response.EnsureSuccessStatusCode();
        return true;
    }

    private async Task AddCookiesToRequest()
    {
        var currentContext = _httpContextAccessor.HttpContext;
        if (currentContext == null) return;

        // Очищаем предыдущие куки
        _httpClient.DefaultRequestHeaders.Remove("Cookie");

        // Добавляем куки из текущего запроса
        var cookies = currentContext.Request.Cookies;
        if (cookies.Any())
        {
            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
            _httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
        }
    }

    private async Task SaveCookiesFromResponse(HttpResponseMessage response)
    {
        var currentContext = _httpContextAccessor.HttpContext;
        if (currentContext == null) return;

        // Сохраняем куки из ответа API
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var setCookie in setCookies)
            {
                // Парсим куки и сохраняем в текущий контекст
                var cookieParts = setCookie.Split(';')[0].Split('=');
                if (cookieParts.Length == 2)
                {
                    currentContext.Response.Cookies.Append(cookieParts[0], cookieParts[1], new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.Now.AddHours(2)
                    });
                }
            }
        }
    }

    public string GetBaseUrl()
    {
        return _apiUrl;
    }


    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
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