using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавляем поддержку контроллеров с представлениями
builder.Services.AddControllersWithViews();

// Добавляем поддержку HttpClient
builder.Services.AddHttpClient<ApiService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["API:url"] ?? "");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30); //Если API не отвечает или отвечает очень медленно, запрос автоматически прервется через
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiService>();

// Настройка Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Путь для перенаправления неавторизованных пользователей на страницу входа
        options.LoginPath = "/Home/Index";

        // Путь для обработки выхода из системы
        options.LogoutPath = "/Home/Logout";

        // Путь для перенаправления при отказе в доступе (403 Forbidden)
        options.AccessDeniedPath = "/Home/Forbidden";

        // Запрещает доступ к куки через JavaScript (защита от XSS-атак)
        options.Cookie.HttpOnly = true;

        // Ограничивает отправку куки только для запросов с того же сайта
        // Strict - куки не отправляются при переходе по ссылкам с других сайтов
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Куки будут отправляться только по HTTPS (обязательно для production)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Время жизни аутентификационной сессии (2 часа)
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // Включение скользящего expiration - время жизни сессии обновляется
        // при каждом запросе пользователя в течение активности
        options.SlidingExpiration = true;

        // Дополнительные полезные опции (можно добавить):

        // options.Cookie.Name = "MyApp.Auth"; // Уникальное имя куки для приложения
        // options.Cookie.Domain = "example.com"; // Домен, для которого действительна куки
        // options.Cookie.Path = "/"; // Путь, для которого действительна куки
        // options.Events // Обработчики событий аутентификации
    });

// Настройка авторизации
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();