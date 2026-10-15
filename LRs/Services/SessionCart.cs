
using System.Collections.Generic;
using LRs.Extensions;
using Microsoft.AspNetCore.Http;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.Services;

public sealed class SessionCart : Cart
{
    private readonly ISession _session;

    public SessionCart(IHttpContextAccessor accessor)
    {
        var httpContext = accessor.HttpContext
            ?? throw new InvalidOperationException(
                "HttpContext недоступен.");

        _session = httpContext.Session;

        var savedCart = _session.Get<Cart>("cart");

        CartItems = savedCart?.CartItems
            ?? new Dictionary<int, CartItem>();
    }

    public override void AddToCart(Bike bike)
    {
        base.AddToCart(bike);
        SaveCart();
    }

    public override void RemoveItems(int id)
    {
        base.RemoveItems(id);
        SaveCart();
    }

    public override void ClearAll()
    {
        base.ClearAll();
        SaveCart();
    }

    private void SaveCart()
    {
        _session.Set<Cart>("cart", this);
    }
}
