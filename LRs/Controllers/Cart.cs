using Microsoft.AspNetCore.Mvc;

namespace LRs.Controllers
{
    public class Cart : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
