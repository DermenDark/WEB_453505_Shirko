using System.Collections.Generic;
using System.Linq;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.Domain.Models;

public class Cart
{
    /// <summary>
    /// Позиции корзины.
    /// Ключ — идентификатор велосипеда.
    /// </summary>
    public Dictionary<int, CartItem> CartItems { get; set; } = new();

    /// <summary>
    /// Общее количество велосипедов с учётом количества каждой позиции.
    /// </summary>
    public int Count =>
        CartItems.Values.Sum(item => item.Count);

    /// <summary>
    /// Общая стоимость корзины.
    /// Bike.Price в проекте имеет тип float.
    /// </summary>
    public float TotalPrice =>
        CartItems.Values.Sum(
            item => item.Item.Price * item.Count);

    /// <summary>
    /// Добавляет велосипед или увеличивает его количество.
    /// </summary>
    public virtual void AddToCart(Bike bike)
    {
        if (CartItems.TryGetValue(bike.Id, out var cartItem))
        {
            cartItem.Count++;
        }
        else
        {
            CartItems[bike.Id] = new CartItem
            {
                Item = bike,
                Count = 1
            };
        }
    }

    /// <summary>
    /// Удаляет велосипед из корзины.
    /// </summary>
    public virtual void RemoveItems(int id)
    {
        CartItems.Remove(id);
    }

    /// <summary>
    /// Полностью очищает корзину.
    /// </summary>
    public virtual void ClearAll()
    {
        CartItems.Clear();
    }
}