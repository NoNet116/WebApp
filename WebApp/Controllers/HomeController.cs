using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Models.View;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        /* Напоминалка 
            ModelState.AddModelError("", "Получено сообщение");
            TempData["ToastMessage"] = "Получено сообщение";
            TempData["ToastType"] = "error";= "success";
         */

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            TempData["ToastMessage"] = "Тык";
            TempData["ToastType"] = "success";

            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


       
    }
}
