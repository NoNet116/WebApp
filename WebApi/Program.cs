using BLL.Extensions;
using BLL.Interfaces;
using BLL.Models;
using BLL.Services;
using DAL;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi.Helpers;
using WebApi.Mapper;

var builder = WebApplication.CreateBuilder(args);

// ��������� ����������� � ��������� ������������ (����� ����� ���� ������������ ����������� � ��������)
builder.Services.AddControllers();

// ��������� ��������� endpoints explorer (����� ��� ��������� �������� API ��� Swagger)
builder.Services.AddEndpointsApiExplorer();

// ������������ Swagger ��������� (��� ������������ � ������������ API ����� ���-���������)
builder.Services.AddSwaggerGen();

// ---------------- ����������� ������� ----------------

// ������������ AutoMapper � ���������� ��������� �������� (BLL � WebApi ����)
builder.Services.AddAutoMapper(
    typeof(BLL.Mapper.BLLMappingProfile),
    typeof(PLLMappingProfile)
);

// �������� ������ ����������� �� ������������ (appsettings.json)
string connection = builder.Configuration.GetConnectionString("DefaultConnection")
                   ?? throw new ArgumentNullException("no string connection");

// ������������ �������� ���� ������ � �������������� SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connection));

// ������������� Identity ��� ������������� (User) � ����� (IdentityRole)
builder.Services.AddIdentity<DAL.Entities.User, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 2;              // ����������� ����� ������
    opts.Password.RequireNonAlphanumeric = false;  // �� ������� ������������
    opts.Password.RequireLowercase = false;        // �� ������� �������� ����
    opts.Password.RequireUppercase = false;        // �� ������� ��������� ����
    opts.Password.RequireDigit = false;            // �� ������� ����
})
.AddEntityFrameworkStores<AppDbContext>()         // �������� ������ Identity � EF Core
.AddDefaultTokenProviders();                      // ��������� ��������� ������� (��������, ��� ������ ������)

// ������������ ������ ��� ������ � ������, ��������� ��������� ���� (��������, User, Administrator)
builder.Services.AddScoped<IRoleService, RoleService>()
    .AddIdentityRoles(RoleType.User, RoleType.Administrator,RoleType.Moderator);

// ������������ ������ ��� ������ � ��������������, ��������� ��������� ���� User
builder.Services.AddScoped<IUserService, UserService>()
    .AddDefaultUserRole(RoleType.User);

builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IArticleTagService, ArticleTagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
// ������������ ���������� ����������� (generic repository) ��� ������ � ����������
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped(typeof(IService<,>), typeof(Service<,>));
//builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/auth/login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, CustomAuthorizationHandler>();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero; // ��������� ������ ������
});

// ---------------- ����� ����������� �������� ----------------

var app = builder.Build();

// ������������� middleware (�������� ��������)

// �������� Swagger ������ � ������ ����������
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();    // ��������� swagger.json
    app.UseSwaggerUI(); // ���-��������� Swagger ��� ������������ API
}

// �������������� ������������� HTTP -> HTTPS
app.UseHttpsRedirection();

app.UseAuthentication();
// �������� middleware ��� ����������� (�������� ���� ������� �� ������ �������� [Authorize])
app.UseAuthorization();

// ���������� ������������� ������������
app.MapControllers();

// ��������� ����������
app.Run();