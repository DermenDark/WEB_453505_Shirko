using LRs.Class;
using LRs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LRs.Controllers
{
    public class Home : Controller
    {
        private List<ListDemo> GetListItems()
        {
            return new List<ListDemo>
            {
                new ListDemo { Id = 1, Name = "Элемент 1" },
                new ListDemo { Id = 2, Name = "Элемент 2" },
                new ListDemo { Id = 3, Name = "Элемент 3" },
                new ListDemo { Id = 4, Name = "Элемент 4" },
                new ListDemo { Id = 5, Name = "Элемент 5" }
            };
        }
        public IActionResult Index(string page = "Index")
        {
            var items = GetListItems();
            var model = new IndexViewModel
            {
                Items = new SelectList(items, "Id", "Name"),
                SelectedId = 2
            };

            ViewData["Title"] = "Лабораторная работа №2";
            if (page == "LR1")
                return View("LR1", model);

            return View("Index");
        }
        [HttpPost]
        public IActionResult Index(IndexViewModel model)
        {
            if (model.SelectedId > 0)
            {
                ViewData["Message"] = $"Выбран элемент с Id: {model.SelectedId}";
            }

            var items = GetListItems();
            model.Items = new SelectList(items, "Id", "Name", model.SelectedId);

            ViewData["Title"] = "Лабораторная работа №2";
            return View("Index", model);
        }
    }
}
