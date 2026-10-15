using LRs.ApiService;
using LRs.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.Controllers;

[Authorize]
[Route("[controller]")]
public class CartController : Controller
{
    private readonly IProductService _productService;

    public CartController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Отображает содержимое корзины.
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        var cart = HttpContext.Session.Get<Cart>("cart")
                   ?? new Cart();

        return View(cart);
    }

    /// <summary>
    /// Добавляет велосипед в корзину.
    /// </summary>
    [HttpGet("add/{id:int}")]
    public async Task<IActionResult> Add(
        int id,
        string? returnUrl = null)
    {
        var response =
            await _productService.GetProductByIdAsync(id);

        if (response.Successfull && response.Data is not null)
        {
            var cart =
                HttpContext.Session.Get<Cart>("cart")
                ?? new Cart();

            cart.AddToCart(response.Data);

            HttpContext.Session.Set("cart", cart);
        }
        else
        {
            TempData["CartError"] =
                "Не удалось добавить велосипед в корзину.";
        }

        // Разрешаем возврат только на локальный адрес.
        if (!string.IsNullOrWhiteSpace(returnUrl)
            && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Product");
    }

    /// <summary>
    /// Удаляет позицию корзины.
    /// </summary>
    [HttpPost("remove/{id:int}")]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var cart =
            HttpContext.Session.Get<Cart>("cart")
            ?? new Cart();

        cart.RemoveItems(id);

        HttpContext.Session.Set("cart", cart);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Очищает корзину.
    /// </summary>
    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        var cart =
            HttpContext.Session.Get<Cart>("cart")
            ?? new Cart();

        cart.ClearAll();

        HttpContext.Session.Set("cart", cart);

        return RedirectToAction(nameof(Index));
    }
}