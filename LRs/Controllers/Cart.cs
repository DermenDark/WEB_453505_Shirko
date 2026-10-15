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
    private readonly Cart _cart;
    public CartController(
        IProductService productService,
        Cart cart)
    {
        _productService = productService;
        _cart = cart;
    }

    /// <summary>
    /// Отображает содержимое корзины.
    /// </summary>
    [HttpGet("")]
    public IActionResult Index()
    {
        return View(_cart);
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
            _cart.AddToCart(response.Data);
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

        _cart.RemoveItems(id);

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Очищает корзину.
    /// </summary>
    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        _cart.ClearAll();

        return RedirectToAction(nameof(Index));
    }
}