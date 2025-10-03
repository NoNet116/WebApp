using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.User;
using WebApp.Services;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Administrator,Moderator")]
    public class UserController(ILogger<UserController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly ApiService _apiService = apiService;

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("{Name}: Главная страница пользователей", User.Identity?.Name);
                var users = await _apiService.GetAsync<List<UserProfileDto>>("/api/Users/All");
                
                if(users ==null)
                    return View();

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

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, [FromBody] Dictionary<string, string> updatedData)
        {
            try
            {
                // Получаем пользователя
                var user = await _apiService.GetAsync<UserViewModel>($"/api/Users/{id.ToString()}");
                if (user == null)
                {
                    TempData["ToastMessage"] = "Пользователь не найден";
                    TempData["ToastType"] = "error";
                    return NotFound(new { message = "Пользователь не найден" });
                }

                // Обновляем свойства
                foreach (var kvp in updatedData)
                {
                    var prop = typeof(UserViewModel).GetProperty(kvp.Key);
                    if (prop != null)
                    {
                        if (prop.PropertyType == typeof(DateOnly))
                        {
                            prop.SetValue(user, DateOnly.Parse(kvp.Value));
                        }
                        else
                        {
                            prop.SetValue(user, kvp.Value);
                        }
                    }
                }

                // Сохраняем изменения через API
              var res =   await _apiService.PutAsync<UserViewModel>($"/api/Users/{id}", user);
                
                return Ok(new { message = "Test: Пользователь успешно обновлён" });
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/Users/{id}");

                if (!result)
                    TempData["ToastMessage"] = "Ошибка при удалении пользователя";
                else
                    TempData["ToastMessage"] = $"Пользователь с ID {id} успешно удалён";
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
            }

            return RedirectToAction("Index"); 
        }


    }


}
