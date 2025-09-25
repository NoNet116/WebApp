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

/* Напоминалка 
            ModelState.AddModelError("", "Получено сообщение");
            TempData["ToastMessage"] = "Получено сообщение";
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
                // Отправляем логин
               
                var loginPayload = new { email = model.Email, password = model.Password };

                var user = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginPayload);

                //Получаем данные текущего пользователя
                //var user = await _apiService.GetAsync<UserProfileDto>("/api/Users/me");
                if (user == null)
                {
                    TempData["ToastMessage"] = "Не удалось получить данные пользователя.";
                    TempData["ToastType"] = "error";
                    return View(model);
                }

                //Создаём Claims
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? user.Email), // User.Identity.Name
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                //Авторизуем пользователя в ASP.NET Core
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                TempData["ToastMessage"] = $"Привет, {user.UserName ?? user.Email}!";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException)
            {
                TempData["ToastMessage"] = "Неверный логин или пароль.";
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
                // Вызываем API logout через общий сервис
                var result = await _apiService.PostAsync<object>("/api/auth/logout", null);

                TempData["ToastMessage"] = "Вы успешно вышли из системы";
                TempData["ToastType"] = "success";
            }
            catch
            {
                // Если API недоступно или вернуло ошибку
                TempData["ToastMessage"] = "Ошибка при выходе из системы";
                TempData["ToastType"] = "error";
            }

            // Очищаем локальные данные
            HttpContext.Session.Clear();

            // Удаляем куки аутентификации
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Удаляем все куки, связанные с аутентификацией
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith(".AspNetCore.") || cookie == "auth" || cookie == "token")
                {
                    Response.Cookies.Delete(cookie);
                }
            }

            // Дополнительно: принудительно удаляем куки
            Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);

            // Если используются дополнительные куки
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
                TempData["ToastMessage"] = "Сессия истекла или пользователь не найден.";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index", "Home");
            }
            var userview = new UserViewModel()
            {
                Id = user.Id,
                Role = user.Role,
                Email = user.Email
            };
            return View("Main", userview);  // например, View(Me.cshtml)
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Регистрация пользователя
        [HttpGet]
        public IActionResult Register()
        {
            var model = new UserRegisterViewModel
            {
                BirthDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
                LastName = "Иванов",
                FirstName = "Иван",
                FatherName = "Иванович",
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

                ModelState.Clear();//Очищаем форму

                TempData["ToastMessage"] = "Пользователь создан";
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
                    TempData["Message"] = "Администратор создан успешно";
                    return Content(responseContent, "application/json");
                }
                else
                {
                    TempData["Message"] = $"Ошибка: {response.StatusCode}";
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
