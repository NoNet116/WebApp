using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.Role;
using WebApp.Models.View.Role.Base;
using WebApp.Services;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleController(ILogger<RoleController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<RoleController> _logger = logger;
        private readonly ApiService _apiService = apiService;

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("{Name}: Главная страница ролей", User.Identity?.Name);

                var rolesdto = await _apiService.GetAsync<List<RoleDto>>("/api/Roles/All");
                if (rolesdto == null)
                {
                    return View();
                }
                var roles = new List<Role>();

                foreach (var role in rolesdto)
                {
                    roles.Add(new Role()
                    {
                        Id = role.Id,
                        Name = role.Name,
                    });
                }

                var model = new RoleViewModel() { Roles = roles };
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
                return View();
            }

            
        }
    }
}
