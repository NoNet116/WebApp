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


        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _apiService.GetAsync<UserInfoDto>("/api/Users/me");

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
               
       
        [HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _apiService.GetAsync<UserInfoDto>("/api/Users/me");

            if (user == null)
            {
                TempData["ToastMessage"] = "������ ������� ��� ������������ �� ������.";
                TempData["ToastType"] = "warning";
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // ���������� ������ �� ����� ����� ����� ������
                var userData = await _apiService.PostAsync<UserInfoDto>(
                    "/api/auth/login",
                    new { email = model.Email, password = model.Password }
                );

                if (userData != null)
                {
                    // ������� claims ��� cookie-��������������
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userData.Email ?? model.Email)
                        // ����� �������� ID, ���� � �.�.
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTime.UtcNow.AddHours(1)
                        });

                    TempData["ToastMessage"] = "�������� ����!";
                    TempData["ToastType"] = "success";

                    return RedirectToAction("Index", "Home");
                }

                // ���� ������
                TempData["ToastMessage"] = "�������� ����� ��� ������.";
                TempData["ToastType"] = "error";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
