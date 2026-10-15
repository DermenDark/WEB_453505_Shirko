using LRs.Extensions;
using Microsoft.AspNetCore.Mvc;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.ViewComponents;

public class CartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var cart =
            ViewContext.HttpContext.Session.Get<Cart>("cart")
            ?? new Cart();

        return View(cart);
    }
}