using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.User;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class UserController(ILogger<UserController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly ApiService _apiService = apiService;

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                var users = await _apiService.GetAsync<List<UserProfileDto>>("/api/Users/All");

                // Маппим в UserViewModel
                var viewModel = users.Select(x => new UserViewModel
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    FatherName = x.FatherName,
                    BirthDate = x.BirthDate,
                    Role = x.Role
                }).ToList();

                // Формируем страницу
                var paged = new PagedResult<UserViewModel>
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalCount = viewModel.Count,
                    Items = viewModel.Skip((page - 1) * pageSize).Take(pageSize).ToList()
                };

                return View(paged);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
                return View(new PagedResult<UserViewModel>());
            }
        }


    }
}
