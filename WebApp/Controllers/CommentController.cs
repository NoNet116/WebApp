using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.Comment;
using WebApp.Models.View.Comment.Base;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class CommentController(ILogger<CommentController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<CommentController> _logger = logger;
        private readonly ApiService _apiService = apiService;
       

        [HttpPost]
        public async Task<IActionResult> Create(CreateCommentViewModel model)
        {
            var comments = await _apiService.PostAsync<CommentBase>($"api/Comment/Add", model);
            return RedirectToAction("Article", "Article", new { id = model.ArticleId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _apiService.DeleteAsync($"api/Comment/{id}");

            //обновить страницу
            //return Redirect(Request.Headers["Referer"].ToString());

            if (result)
                return Json(new { success = true, id });
            else
                return Json(new { success = false, message = "Ошибка при удалении" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string message)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "пустое сообщение" });
            }
            var model = new CommentEditViewModel { Id = id, Message = message };

            var result = await _apiService.PutAsync<ApiResponse<CommentEditViewModel>>($"api/Comment/Edit", new { CommentId = id, Message = message });

            if (result.Success)
                return Json(new { success = true, id, message, updatedAt = DateTime.Now.ToString("dd.MM.yyyy HH:mm") });
            else
                return Json(new { success = false, message = "Ошибка при редактировании" });
        }

    }
}
