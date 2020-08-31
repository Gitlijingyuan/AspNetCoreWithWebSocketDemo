using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreWithWebSocketDemo.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction(nameof(HomeController.WebSocketDemo));
        }

        [HttpGet]
        public IActionResult WebSocketDemo()
        {
            return View();
        }


    }
}
