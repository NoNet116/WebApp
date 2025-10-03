using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Models.View;
using WebApp.Models.View.Article;
using WebApp.Models.View.Comment;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ArticleController(ILogger<ArticleController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<ArticleController> _logger = logger;
        private readonly ApiService _apiService = apiService;

        [HttpGet("Articles")]
        public async Task<IActionResult> Index(int startIndex = 0, int count = 10)
        {
            try
            {
                _logger.LogInformation("{Name}: Главная страница статьи", User.Identity?.Name);
                var articles = await _apiService.GetAsync<List<ArticleViewModel>>($"/api/Article/{startIndex}/{count}");

                return View(articles);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateArticle(RegisterArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            try
            {
                var result = await _apiService.PostAsync<ApiResponse<ArticleViewModel>>("/api/Article", model);
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "error";
                TempData.Keep();
            }
            return RedirectToAction("Index");
        }

        [HttpGet("Article-{id}")]
        public async Task<IActionResult> Article(uint id)
        {
            var result = await _apiService.GetAsync<ArticleViewModel>($"api/Article/{id}");

            if (result == null)
            {
                return RedirectToAction("Index");
            }

            // Создаем анонимный объект для JSON тела
            var requestBody = new { articleId = id, count = 0 };
            var comments = await _apiService.PostAsync<CommentViewModel<int>>($"api/Comment/Get", requestBody);

            if (comments != null)
            {
                result.Comments = comments!.Comments;
            }
            return View(result);
        }

        // Edit action
        public async Task<IActionResult> Edit(uint id)
        {
            // Проверка прав доступа
            var article = await _apiService.GetAsync<ArticleViewModel>($"api/Article/{id}");

            if (article == null)
            {
                TempData["ToastMessage"] = "Статья не найдена";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Administrator") && article.AuthorId != currentUserId)
            {
                TempData["ToastMessage"] = "У вас нет прав для редактирования этой статьи";
                TempData["ToastType"] = "error";
                return RedirectToAction("Article", new { id });
            }

            var editModel = new EditArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Content = article.Content,
                Tags = article.Tags
            };
            return View(editModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditArticleViewModel model)
        {
            // Проверка прав доступа на стороне Api?
            /*var article = await _apiService.GetAsync<ArticleViewModel>($"api/Article/{model.Id}");

            if (article == null)
            {
                TempData["ToastMessage"] = "Статья не найдена";
                TempData["ToastType"] = "error";
                return RedirectToAction("Index");
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Administrator") && article.AuthorId != currentUserId)
            {
                TempData["ToastMessage"] = "У вас нет прав для редактирования этой статьи";
                TempData["ToastType"] = "error";
                return RedirectToAction("Article", new { model.Id });
            }*/

            try
            {
                var article = await _apiService.PutAsync<ApiResponse<EditArticleViewModel>>($"api/Article/Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("",ex.Message);
                return View(model);
            }
            
            return RedirectToAction("Article", new { id = model.Id });
        }

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

        [HttpGet]
        public async Task<IActionResult> Search(SearchViewModel model)
        {
            var articles = await _apiService.GetAsync<List<ArticleViewModel>>($"/api/Article");
            if(articles == null)
            {
                return View("Index");
            }

            IEnumerable<ArticleViewModel> result = articles;

            if (!string.IsNullOrEmpty(model.Author))
            {
                result = result.Where(x =>
                    !string.IsNullOrEmpty(x.AuthorName) &&
                    x.AuthorName.Contains(model.Author, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(model.Title))
            {
                result = result.Where(x =>
                    !string.IsNullOrEmpty(x.Title) &&
                    x.Title.Contains(model.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(model.Tag))
            {
                result = result.Where(x =>
                    x.Tags != null &&
                    x.Tags.Any(t => t.Contains(model.Tag, StringComparison.OrdinalIgnoreCase)));
            }

            if (model.DateTime.HasValue)
            {
                result = result.Where(x => x.CreatedAt.Date == model.DateTime.Value.Date);
            }

            return View("Index", result.ToList());
        }
    }
}