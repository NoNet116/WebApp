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

    public ApiService(IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _apiUrl = config["API:url"] ?? throw new ArgumentNullException("API:url");

        _cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_apiUrl)
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddCookiesFromCurrentContext();
        var response = await _httpClient.GetAsync(endpoint);
        SaveCookiesToContext();
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? payload)
    {
        AddCookiesFromCurrentContext();

        HttpContent? content = null;
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.PostAsync(endpoint, content);
        SaveCookiesToContext();
        return await HandleResponse<T>(response);
    }

    public async Task<T?> PutAsync<T>(string endpoint, object? payload)
    {
        AddCookiesFromCurrentContext();

        HttpContent? content = null;
        if (payload != null)
        {
            var json = JsonSerializer.Serialize(payload);
            content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.PutAsync(endpoint, content);
        SaveCookiesToContext();
        return await HandleResponse<T>(response);
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        AddCookiesFromCurrentContext();
        var response = await _httpClient.DeleteAsync(endpoint);
        SaveCookiesToContext();
        response.EnsureSuccessStatusCode();
        return true;
    }

    public string GetBaseUrl() => _apiUrl;

    #region Private Helpers

    private void AddCookiesFromCurrentContext()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        foreach (var cookie in context.Request.Cookies)
        {
            _cookieContainer.Add(new Uri(_apiUrl), new Cookie(cookie.Key, cookie.Value));
        }
    }

    private void SaveCookiesToContext()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var cookies = _cookieContainer.GetCookies(new Uri(_apiUrl)).Cast<Cookie>();
        foreach (var cookie in cookies)
        {
            context.Response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions
            {
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
                SameSite = SameSiteMode.Strict,
                Expires = cookie.Expires == DateTime.MinValue ? null : cookie.Expires
            });
        }
    }

    private async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"API StatusCode: {response.StatusCode}. Ответ сервера: {responseContent}");
        }

        return string.IsNullOrWhiteSpace(responseContent)
            ? default
            : JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    #endregion
}
