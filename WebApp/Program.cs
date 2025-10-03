using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services;
using NLog;
using NLog.Web;

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
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Куки будут отправляться только по HTTPS (обязательно для production)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Время жизни аутентификационной сессии (2 часа)
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // Включение скользящего expiration - время жизни сессии обновляется
        // при каждом запросе пользователя в течение активности
        options.SlidingExpiration = true;

        // Дополнительные полезные опции (можно добавить):
        // options.Cookie.Name = "MyApp.Auth";
        // options.Cookie.Domain = "example.com";
        // options.Cookie.Path = "/";
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

// Настройка NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    // Глобальный обработчик ошибок (production)
    app.UseExceptionHandler("/Home/Error");

    // Включаем HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}
else
{
    // В режиме разработки можно включить страницу с подробной информацией об ошибке
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Глобальный middleware для перехвата непредвиденных исключений
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        // Логируем непредвиденные ошибки
        var logger = LogManager.GetCurrentClassLogger();
        logger.Error(ex, "Произошло необработанное исключение");

        // Перенаправляем пользователя на страницу
        context.Response.Redirect("/Home/ErrorPage");
    }
});

// Маршрутизация контроллеров
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
