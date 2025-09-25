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
using WebApp.Models.View.Role.Base;
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
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // ���������� �����
               
                var loginPayload = new { email = model.Email, password = model.Password };

                var user = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginPayload);

                //�������� ������ �������� ������������
                //var user = await _apiService.GetAsync<UserProfileDto>("/api/Users/me");
                if (user == null)
                {
                    TempData["ToastMessage"] = "�� ������� �������� ������ ������������.";
                    TempData["ToastType"] = "error";
                    return View(model);
                }

                //������ Claims
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email), // User.Identity.Name
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                //���������� ������������ � ASP.NET Core
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                TempData["ToastMessage"] = $"������, {user.UserName ?? user.Email}!";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                TempData["ToastMessage"] = "�������� ����� ��� ������.";
                TempData["ToastType"] = "error";
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

        public IActionResult ForgotPassword()
        {
            return View();
        }


    }
}
