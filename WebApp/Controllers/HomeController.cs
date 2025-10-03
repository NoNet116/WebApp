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

                var user = User?.Identity;
                if (user?.IsAuthenticated == false)
                    return View();

                var userDto = await _apiService.GetAsync<UserDto>("/api/Users/me");

                if (userDto != null)
                {
                    _logger.LogInformation("{Name}: Главная страница профиля", user?.Name);
                    
                    var userview = new UserViewModel()
                    {
                        Id = userDto.Id,
                        Role = userDto.Role,
                        Email = userDto.Email
                    };
                    return View("Main", userview);
                }

                _logger.LogDebug("Авторизованный пользователь не найден: {Name}", user?.Name);
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
            _logger.LogInformation("Попытка авторизации: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Не корректные данные для авторизации: {Email}", model.Email);
                return View(model);
            }

            try
            {
                // Отправляем логин

                var loginPayload = new { email = model.Email, password = model.Password };

                var user = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginPayload);

                if (user == null)
                {
                    _logger.LogInformation("Пользователь не найден: {Email}", model.Email);

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

                _logger.LogInformation("Пользователь {Email} успешно авторизовался, роль: {Role}",
                    user.Email, user.Role);

                TempData["ToastMessage"] = $"Привет, {user.UserName ?? user.Email}!";
                TempData["ToastType"] = "success";

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Неверный логин или пароль: {Email}", model.Email);

                TempData["ToastMessage"] = "Неверный логин или пароль.";
                TempData["ToastType"] = "error";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при входе в систему: {Email}", model.Email);

                TempData["ToastMessage"] = "Произошла ошибка при входе в систему.";
                TempData["ToastType"] = "error";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name ?? "Unknown";

            _logger.LogInformation("Выход из системы пользователя: {UserEmail}", userEmail);

            try
            {
                // Вызываем API logout через общий сервис
                var result = await _apiService.PostAsync<object>("/api/auth/logout", null);

                _logger.LogInformation("Успешный выход пользователя из API: {UserEmail}", userEmail);

                TempData["ToastMessage"] = "Вы успешно вышли из системы";
                TempData["ToastType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ошибка выхода пользователя из API: {UserEmail}", userEmail);

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

            _logger.LogInformation("Пользователь  {UserEmail} успешно вышел из системы", userEmail);

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
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("Error occurred with RequestId: {RequestId}", requestId);

            return View(new ErrorViewModel { RequestId = requestId });
        }

        #region Регистрация пользователя
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogDebug("Home/Register GET action called");

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

                ModelState.Clear();//Очищаем форму

                _logger.LogInformation("User {Email} successfully registered", model.Email);

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

                    TempData["Message"] = "Администратор создан успешно";
                    return Content(responseContent, "application/json");
                }
                else
                {
                    _logger.LogWarning("Failed to create administrator. Status: {StatusCode}", response.StatusCode);

                    TempData["Message"] = $"Ошибка: {response.StatusCode}";
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
            string msg = "Что-то пошло не так. Пожалуйста, попробуйте позже.";

            return View(new ErrorViewModel {RequestId = requestId, Message = msg });
           
        }

        /// <summary>
        /// Метод для принудительной ошибки
        /// </summary>
        /// <returns>DivideByZeroException</returns>
        public IActionResult Crash()
        {
            _logger.LogWarning("Пользователь вызвал тестовое исключение через /Home/Crash");

            int x = 0;
            int y = 10 / x;

            return Content($"Результат: {y}");
        }

    }
}