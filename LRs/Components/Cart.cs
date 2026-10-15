
using LRs.Extensions;
using Microsoft.AspNetCore.Mvc;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.Components;

public class CartSummary : ViewComponent
{
    private readonly Cart _cart;

    public CartSummary(Cart cart)
    {
        _cart = cart;
    }

    public IViewComponentResult Invoke()
    {
        return View(_cart);
    }
}