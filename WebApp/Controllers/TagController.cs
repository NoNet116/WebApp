using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models.View.Article;
using WebApp.Models.View.Tag;
using WebApp.Services;

namespace WebApp.Controllers
{
    public class TagController(ILogger<TagController> logger, ApiService apiService) : Controller
    {
        private readonly ILogger<TagController> _logger = logger;
        private readonly ApiService _apiService = apiService;
    
        public async Task<IActionResult> Index()
        {
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
    }
}
