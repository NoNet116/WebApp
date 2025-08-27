using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class ArticleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
