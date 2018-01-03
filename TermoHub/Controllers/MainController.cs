using Microsoft.AspNetCore.Mvc;

namespace TermoHub
{
    public class MainController : Controller
    {
        // GET: /
        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction("List", "Device");
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
