using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApp.Models;
using WebApp.Models.View;
using WebApp.Models.View.User;
using WebApp.Services;

/* ����������� 
            ModelState.AddModelError("", "�������� ���������");
            TempData["ToastMessage"] = "�������� ���������";
            TempData["ToastType"] = "error";= "success";
*/

namespace WebApp.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _apiService;

        public HomeController(ILogger<HomeController> logger,
                              ApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        #region index & Logout
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserProfileDto>>("/api/Users/All");
                TempData["AdminExist"] = users?.Any(x => x.Role == "Administrator") ?? false;

                var user = await _apiService.GetAsync<UserDto>("/api/Users/me");

                if (user != null)
                {
                    var userview = new UserViewModel()
                    {
                        Id = user.Id,
                        Role = user.Role,
                        Email = user.Email
                    };
                    return View("Main", userview);
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);
                // ������� ��������� HttpClient ��� ������
                using var loginClient = new HttpClient();
                loginClient.BaseAddress = new Uri(_apiService.GetBaseUrl());
                loginClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var loginData = new
                {
                    email = model.Email,
                    password = model.Password
                };

                var json = JsonSerializer.Serialize(loginData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await loginClient.PostAsync("/api/auth/login", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ToastMessage"] = "�������� ����� ��� ������.";
                    TempData["ToastType"] = "error";
                    return View(model);
                }
                var user = await loginClient.GetAsync("/api/Users/me");
                var responseContent2 = await response.Content.ReadAsStringAsync();

                // ��������� ���� �� ������ ������
                if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
                {
                    foreach (var setCookie in setCookies)
                    {
                        var cookieParts = setCookie.Split(';')[0].Split('=');
                        if (cookieParts.Length == 2)
                        {
                            Response.Cookies.Append(cookieParts[0], cookieParts[1], new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Expires = DateTimeOffset.Now.AddHours(2)
                            });
                        }
                    }
                }

                // �������� ������ ������������ � ��������� �� null
                var userData = JsonSerializer.Deserialize<UserDto>(responseContent2, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (userData != null)
                {
                    // ���������� �������� claims � ��������� �� null
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userData.UserName ?? model.Email),
                new Claim(ClaimTypes.NameIdentifier, userData.Id?.ToString() ?? "unknown"),
                new Claim(ClaimTypes.Role, userData.Role ?? "User")
            };

                    // ��������� ������ �� claims, ������� ����� ��������
                    if (!string.IsNullOrEmpty(userData.Email))
                        claims.Add(new Claim(ClaimTypes.Email, userData.Email));

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(2)
                        });

                    // ��������� ���������� � ������ ��� �������������� ������������
                    HttpContext.Session.SetString("UserEmail", userData.Email ?? "");
                    HttpContext.Session.SetString("UserId", userData.Id?.ToString() ?? "");
                    HttpContext.Session.SetString("UserRole", userData.Role ?? "");

                    TempData["ToastMessage"] = "�������� ����!";
                    TempData["ToastType"] = "success";
                    return RedirectToAction("Index", "Home");
                }

                TempData["ToastMessage"] = "������ ��� ��������� ������ ������������.";
                TempData["ToastType"] = "error";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = $"������: {ex.Message}";
                TempData["ToastType"] = "error";
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // �������� API logout ����� ����� ������
                var result = await _apiService.PostAsync<object>("/api/auth/logout", null);

                TempData["ToastMessage"] = "�� ������� ����� �� �������";
                TempData["ToastType"] = "success";
            }
            catch
            {
                // ���� API ���������� ��� ������� ������
                TempData["ToastMessage"] = "������ ��� ������ �� �������";
                TempData["ToastType"] = "error";
            }

            // ������� ��������� ������
            HttpContext.Session.Clear();

            // ������� ���� ��������������
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // ������� ��� ����, ��������� � ���������������
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith(".AspNetCore.") || cookie == "auth" || cookie == "token")
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            // �������������: ������������� ������� ����
            Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);

            // ���� ������������ �������������� ����
            Response.Cookies.Delete(".AspNetCore.Session");
            Response.Cookies.Delete(".AspNetCore.Antiforgery");

            return RedirectToAction("Index");
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _apiService.GetAsync<UserDto>("/api/Users/me");

            if (user == null)
            {
                TempData["ToastMessage"] = "������ ������� ��� ������������ �� ������.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            var userview = new UserViewModel()
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email
            };
            return View("Main", userview);  // ��������, View(Me.cshtml)
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region ����������� ������������
        [HttpGet]
        public IActionResult Register()
        {
            var model = new UserRegisterViewModel
            {
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                LastName = "������",
                FirstName = "����",
                FatherName = "��������",
                UserName = "Ivanov",
                Email = "i@example.com"

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                var result = await _apiService.PostAsync<UserRegisterViewModel>(
                    "/api/Users/Create",
                    new
                    {
                        email = model.Email,
                        password = model.Password,
                        username = model.UserName,
                        lastname = model.LastName,
                        firstname = model.FirstName,
                        birthdate = model.BirthDate,
                        fathername = model.FatherName
                    }
                );

                ModelState.Clear();//������� �����

                TempData["ToastMessage"] = "������������ ������";
                TempData["ToastType"] = "success";

                var newmodel = new UserRegisterViewModel
                {
                    BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-18))
                };

                return View(newmodel);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";

                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> CreateAdmin()
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri(_apiService.GetBaseUrl());
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/Users/CreateAdministrator", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "������������� ������ �������";
                    return Content(responseContent, "application/json");
                }
                else
                {
                    TempData["Message"] = $"������: {response.StatusCode}";
                    return BadRequest(responseContent);
                }
            }
            catch (Exception ex )
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Forbidden()
        {
            return View();
        }

    }
}
