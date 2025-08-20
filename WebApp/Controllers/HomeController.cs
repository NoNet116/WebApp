using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View;

/* Напоминалка 
            ModelState.AddModelError("", "Получено сообщение");
            TempData["ToastMessage"] = "Получено сообщение";
            TempData["ToastType"] = "error";= "success";
*/

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private string  ApiUrl = null!; 

        public HomeController(ILogger<HomeController> logger,
                              IHttpClientFactory httpClientFactory,
                              IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
          var  apiUrl = string.IsNullOrWhiteSpace(_configuration["API:url"])
            ? throw new ArgumentNullException("API:url не задана"): _configuration["API:url"]; // Получаем URL из конфига
            ApiUrl = apiUrl;
        }


        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var payload = new { email = model.Email, password = model.Password };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{ApiUrl}/api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Успешный вход!";
                TempData["ToastType"] = "success";

            }
            else
            {
                TempData["ToastMessage"] = $"Ошибка: {response.StatusCode}";
                TempData["ToastType"] = "error";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{ApiUrl}/api/auth/logout", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "Вы успешно вышли из системы";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "Ошибка при выходе";
                TempData["ToastType"] = "error";
            }

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
