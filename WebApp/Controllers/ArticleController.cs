using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Models.View.Article;
using WebApp.Models.View.Article.Base;
using WebApp.Models.View.Comment;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ArticleController(ILogger<ArticleController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<ArticleController> _logger = logger;
        private readonly ApiService _apiService = apiService;
        
        [HttpGet]   
        public async Task<IActionResult> Index(int startIndex = 0, int count = 10)
        {
            try
            {
                var articles = await _apiService.GetAsync<List<ArticleViewModel>>($"/api/Article/{startIndex}/{count}");

                return View(articles);
            }
            catch (Exception ex)
            {

                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle(RegisterArticleViewModel model)
        {
            if (!ModelState.IsValid) { 
                return RedirectToAction("Index");
            }
            try
            {
                var result = await _apiService.PostAsync<ApiResponse<ArticleViewModel>>("/api/Article",model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
                TempData.Keep();
            }
            return RedirectToAction("Index");
        }

        [HttpGet] 
        public async Task<IActionResult> Article(uint id)
        {
            var result = await _apiService.GetAsync<ArticleViewModel>($"api/Article/{id}");

            if (result == null)
            {
                return RedirectToAction("Index");
            }

            // Создаем анонимный объект для JSON тела
            var requestBody = new { articleId = id, count = 0 };
            var comments = await _apiService.PostAsync<CommentViewModel>($"api/Comment/Get", requestBody);

            if (comments != null)
            {
                result.Comments = comments!.Comments;
                
            }
            return View(result);
        }

        /*// Edit action
        [Authorize]
        public async Task<IActionResult> Edit(uint id)
        {

            // Проверка прав доступа
            var article = await _articleService.GetByIdAsync(id);
            if (article == null) return NotFound();

            if (!User.IsInRole("Admin") && User.Identity.Name != article.AuthorName)
                return Forbid();

            return View(article);
        }*/

       [HttpPost]
        public async Task<IActionResult> Delete(uint id)
        {
            var article = await _apiService.GetAsync<ArticleViewModel>($"api/Article/{id}");
            if (article == null)
            {
                TempData["ErrorMessage"] = "Статья не найдена";
                return RedirectToAction("Index");
            }

            // Получаем ID текущего пользователя
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Проверяем, что у статьи есть AuthorId и сравниваем с ID текущего пользователя
            if (!User.IsInRole("Administrator") && article.AuthorId != currentUserId)
                return Forbid();

            var result = await _apiService.DeleteAsync($"/api/Article/{id}");

            return RedirectToAction("Index");
        }

    }
}
