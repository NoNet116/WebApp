using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View.Article;
using WebApp.Models.View.Article.Base;
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
    }
}
