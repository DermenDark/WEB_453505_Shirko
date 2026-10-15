using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.Domain.Models
{
    public class CartItem
    {
        // Велосипед, добавленный в корзину.
        public Bike Item { get; set; } = null!;

        // Количество экземпляров этого велосипеда.
        public int Count { get; set; } = 1;
    }
}
