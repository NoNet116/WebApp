using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View;
using WebApp.Models.View.User;

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
            ? throw new ArgumentNullException("API:url �� ������"): _configuration["API:url"]; // �������� URL �� �������
            ApiUrl = apiUrl;
        }


        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        /*[HttpPost]
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
                TempData["ToastMessage"] = "�������� ����!";
                TempData["ToastType"] = "success";

            }
            else
            {
                TempData["ToastMessage"] = $"������: {response.StatusCode}";
                TempData["ToastType"] = "error";
            }

            return View(model);
        }
*/
        [HttpGet]
        public async Task<IActionResult> Me()
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiUrl}/api/Users/me");

            // ��������� ���� � ������ � API
            var cookie = HttpContext.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserInfoDto>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var userview = new UserViewModel()
                {
                    Id = user.Id,
                    Role = user.Role,
                    Email = user.Email
                };
                return View("Main", userview); // ��������� ������ �� �������� Me.cshtml
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                TempData["ToastMessage"] = "������ �������, ��������� ���� �����.";
                TempData["ToastType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            TempData["ToastMessage"] = $"������: {response.StatusCode}";
            TempData["ToastType"] = "error";
            return RedirectToAction("Index", "Home");
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
                // ��������� ������ ������������ �� ������ API (���� API ���������� JSON � �����������)
                var responseContent = await response.Content.ReadAsStringAsync();
                var userData = JsonSerializer.Deserialize<UserInfoDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // ������� claims ��� cookie-��������������
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userData?.Email ?? model.Email),
            //new Claim("UserId", userData?.Id.ToString() ?? "")
            // ������ ������ claim-�, ���� �����
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true, // "��������� ����"
                        ExpiresUtc = DateTime.UtcNow.AddHours(1)
                    });

                TempData["ToastMessage"] = "�������� ����!";
                TempData["ToastType"] = "success";

                ViewBag.Url = $"{ApiUrl}/api/Users/me";
                TempData["url"] = $"{ApiUrl}/api/Users/me";
                return RedirectToAction("Me", "Home"); // ����� ����� �������� �� �������
            }
            else
            {
                TempData["ToastMessage"] = $"������: {response.StatusCode}";
                TempData["ToastType"] = "error";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync($"{ApiUrl}/api/auth/logout", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["ToastMessage"] = "�� ������� ����� �� �������";
                TempData["ToastType"] = "success";
            }
            else
            {
                TempData["ToastMessage"] = "������ ��� ������";
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
