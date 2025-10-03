using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
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

                var user = User?.Identity;
                if (user?.IsAuthenticated == false)
                    return View();

                var userDto = await _apiService.GetAsync<UserDto>("/api/Users/me");

                if (userDto != null)
                {
                    _logger.LogInformation("{Name}: ������� �������� �������", user?.Name);
                    
                    var userview = new UserViewModel()
                    {
                        Id = userDto.Id,
                        Role = userDto.Role,
                        Email = userDto.Email
                    };
                    return View("Main", userview);
                }

                _logger.LogDebug("�������������� ������������ �� ������: {Name}", user?.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home/Index while checking user authentication");

                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
            }

            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            _logger.LogInformation("������� �����������: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("�� ���������� ������ ��� �����������: {Email}", model.Email);
                return View(model);
            }

            try
            {
                // ���������� �����

                var loginPayload = new { email = model.Email, password = model.Password };

                var user = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginPayload);

                if (user == null)
                {
                    _logger.LogInformation("������������ �� ������: {Email}", model.Email);

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

                _logger.LogInformation("������������ {Email} ������� �������������, ����: {Role}",
                    user.Email, user.Role);

                TempData["ToastMessage"] = $"������, {user.UserName ?? user.Email}!";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "�������� ����� ��� ������: {Email}", model.Email);

                TempData["ToastMessage"] = "�������� ����� ��� ������.";
                TempData["ToastType"] = "error";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "����������� ������ ��� ����� � �������: {Email}", model.Email);

                TempData["ToastMessage"] = "��������� ������ ��� ����� � �������.";
                TempData["ToastType"] = "error";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name ?? "Unknown";

            _logger.LogInformation("����� �� ������� ������������: {UserEmail}", userEmail);

            try
            {
                // �������� API logout ����� ����� ������
                var result = await _apiService.PostAsync<object>("/api/auth/logout", null);

                _logger.LogInformation("�������� ����� ������������ �� API: {UserEmail}", userEmail);

                TempData["ToastMessage"] = "�� ������� ����� �� �������";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "������ ������ ������������ �� API: {UserEmail}", userEmail);

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

            _logger.LogInformation("������������  {UserEmail} ������� ����� �� �������", userEmail);

            return RedirectToAction("Index");
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Me()
        {
            _logger.LogDebug("Home/Me action called");

            var user = await _apiService.GetAsync<UserDto>("/api/Users/me");

            if (user == null)
            {
                _logger.LogWarning("User not found in Home/Me - session may have expired");

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
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error occurred with RequestId: {RequestId}", requestId);

            return View(new ErrorViewModel { RequestId = requestId });
        }

        #region ����������� ������������
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogDebug("Home/Register GET action called");

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
            _logger.LogInformation("Registration attempt for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Registration model validation failed for email: {Email}", model.Email);
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

                _logger.LogInformation("User {Email} successfully registered", model.Email);

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
                _logger.LogError(ex, "Registration failed for email: {Email}", model.Email);

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
            _logger.LogInformation("CreateAdmin action called");

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
                    _logger.LogInformation("Administrator created successfully");

                    TempData["Message"] = "������������� ������ �������";
                    return Content(responseContent, "application/json");
                }
                else
                {
                    _logger.LogWarning("Failed to create administrator. Status: {StatusCode}", response.StatusCode);

                    TempData["Message"] = $"������: {response.StatusCode}";
                    return BadRequest(responseContent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAdmin action");

                return BadRequest(ex.Message);
            }
        }

        public IActionResult Forbidden()
        {
            _logger.LogWarning("Access denied for user: {User} to resource: {Path}",
                User.Identity?.Name, HttpContext.Request.Path);

            return View();
        }

        public IActionResult ForgotPassword()
        {
            _logger.LogDebug("ForgotPassword page accessed");
            return View();
        }

        public IActionResult ErrorPage()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            string msg = "���-�� ����� �� ���. ����������, ���������� �����.";

            return View(new ErrorViewModel {RequestId = requestId, Message = msg });
           
        }

        /// <summary>
        /// ����� ��� �������������� ������
        /// </summary>
        /// <returns>DivideByZeroException</returns>
        public IActionResult Crash()
        {
            _logger.LogWarning("������������ ������ �������� ���������� ����� /Home/Crash");

            int x = 0;
            int y = 10 / x;

            return Content($"���������: {y}");
        }

    }
}