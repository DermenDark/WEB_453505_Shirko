using Microsoft.AspNetCore.Mvc;

namespace LRs.Controllers
{
    public class Account : Controller
    {
        public IActionResult LogOut()
        {
            return View();
        }
    }
}
