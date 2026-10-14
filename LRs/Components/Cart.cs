using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace LRs.Components
{
    public class CartSummary : ViewComponent
    {
        private readonly IRepository _repository;

        public CartSummary(IRepository repo)
        {
            _repository = repo;
        }

        public IViewComponentResult Invoke()
        {
            var cartItems = _repository.GetAll();

            if (cartItems == null || !cartItems.Any())
            {
                cartItems = new List<CartItem>
                {
                    new CartItem { Id = 1, Price = 100 },
                    new CartItem { Id = 2, Price = 200 }
                };
            }

            var model = new CartSummaryViewModel
            {
                TotalCount = cartItems.Count(),
                TotalAmount = cartItems.Sum(x => x.Price)
            };

            return View(model);
        }
    }

    public class CartSummaryViewModel
    {
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
    public interface IRepository
    {
        IEnumerable<CartItem> GetAll();
    }
    public class CartRepository : IRepository
    {
        public IEnumerable<CartItem> GetAll()
        {
            return new List<CartItem>
            {
                new CartItem { Id = 1, Price = 100 },
                new CartItem { Id = 2, Price = 200 }
            };
        }
    }
}