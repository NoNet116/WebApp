using Microsoft.AspNetCore.Mvc;
using WebApp.Models.View.Article;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class ArticleController(ILogger<ArticleController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<ArticleController> _logger = logger;
        private readonly ApiService _apiService = apiService;
    
        public async Task<IActionResult> Index(string find)
        {
            try
            {
                var articles = await _apiService.GetAsync<List<ArticleViewModel>>($"/api/Article?title={find}");

                return View(articles);
            }
            catch (Exception ex)
            {

                return View();
            }
        }
    }
}
