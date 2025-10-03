using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using WebApp.Services;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// ��������� ��������� ������������ � ���������������
builder.Services.AddControllersWithViews();

// ��������� ��������� HttpClient
builder.Services.AddHttpClient<ApiService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(config["API:url"] ?? "");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30); //���� API �� �������� ��� �������� ����� ��������, ������ ������������� ��������� �����
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApiService>();

// ��������� Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // ���� ��� ��������������� ���������������� ������������� �� �������� �����
        options.LoginPath = "/Home/Index";

        // ���� ��� ��������� ������ �� �������
        options.LogoutPath = "/Home/Logout";

        // ���� ��� ��������������� ��� ������ � ������� (403 Forbidden)
        options.AccessDeniedPath = "/Home/Forbidden";

        // ��������� ������ � ���� ����� JavaScript (������ �� XSS-����)
        options.Cookie.HttpOnly = true;

        // ������������ �������� ���� ������ ��� �������� � ���� �� �����
        options.Cookie.SameSite = SameSiteMode.Strict;

        // ���� ����� ������������ ������ �� HTTPS (����������� ��� production)
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // ����� ����� ������������������ ������ (2 ����)
        options.ExpireTimeSpan = TimeSpan.FromHours(2);

        // ��������� ����������� expiration - ����� ����� ������ �����������
        // ��� ������ ������� ������������ � ������� ����������
        options.SlidingExpiration = true;

        // �������������� �������� ����� (����� ��������):
        // options.Cookie.Name = "MyApp.Auth";
        // options.Cookie.Domain = "example.com";
        // options.Cookie.Path = "/";
    });

// ��������� �����������
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

// ��������� NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    // ���������� ���������� ������ (production)
    app.UseExceptionHandler("/Home/Error");

    // �������� HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}
else
{
    // � ������ ���������� ����� �������� �������� � ��������� ����������� �� ������
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ���������� middleware ��� ��������� �������������� ����������
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        // �������� �������������� ������
        var logger = LogManager.GetCurrentClassLogger();
        logger.Error(ex, "��������� �������������� ����������");

        // �������������� ������������ �� ��������
        context.Response.Redirect("/Home/ErrorPage");
    }
});

// ������������� ������������
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
