using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.Article;
using WebApp.Models.View.Tag;
using WebApp.Models.View.Tag.Base;
using WebApp.Services;
using static WebApp.Controllers.TagController;

namespace WebApp.Controllers
{
    public class TagController(ILogger<TagController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<TagController> _logger = logger;
        private readonly ApiService _apiService = apiService;
    
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("{Name}: Главная страница тегов", User.Identity?.Name);
            var tags = await _apiService.GetAsync<List<TagViewModel>>($"/api/Tag");
            return View(tags);
        }

        [HttpPost, Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _apiService.DeleteAsync($"/api/Tag/{id}");
                if (result)
                {
                    TempData["ToastMessage"] = "Тег удален.";
                }
                else
                {
                    TempData["ToastMessage"] = "Тег не удален.";
                }
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] TagViewModel model)
        {
            // Проверка валидности
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Данные не переданы" });
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest(new { success = false, message = "Название тега не должно быть пустым" });
            }

            if (string.IsNullOrWhiteSpace(model.Id))
            {
                return BadRequest(new { success = false, message = "Поле Id не должно быть пустым" });
            }

            try
            {
                // Отправка на API
                var result = await _apiService.PutAsync<TagViewModel>("/api/Tag/update", model);

                // Предположим, API возвращает обновлённый объект или успех
                return Ok(new { success = true, message = "Изменения сохранены", data = result });
            }
            catch (Exception ex)
            {
                // Логируем ошибку, возвращаем JSON с ошибкой
                _logger.LogError(ex, "Ошибка при обновлении тега с Id {TagId}", model.Id);
                return StatusCode(500, new { success = false, message = "Ошибка при сохранении", details = ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagBase model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                return Json(new { success = false, message = "Название обязательно" });
            }
            if (string.IsNullOrEmpty(model.Description))
            {
               model.Description = string.Empty;
            }

            try
            {
                
                var apiResponse = await _apiService.PostAsync<ApiResponse<bool>>("/api/Tag/Create", model);

                if (apiResponse!.Success && apiResponse.Data)
                {
                    return Json(new { success = true, message = "[Create] Тег создан успешно" });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = apiResponse.Errors?.FirstOrDefault() ?? "Не удалось создать тег"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании тега");
                return Json(new { success = false, message = "Внутренняя ошибка сервера" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var tags = await _apiService.GetAsync<ApiResponse<List<TagViewModel>>>($"/api/Tag/by-name?name={query}");
            if (!tags!.DataIsNull)
            {                
                return Ok(tags.Data!.Select(x => new SelectorItem {Text =x.Name,Value =x.Name }));
               
            }

            return Ok(Enumerable.Empty<string>());
        }
    }
}
