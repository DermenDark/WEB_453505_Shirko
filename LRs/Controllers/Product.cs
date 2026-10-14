using Microsoft.AspNetCore.Mvc;
using LRs.Services.BikeProductService;
using LRs.Services.BikeCategoryService;
using LRs.ApiService;

namespace WEB_453505_Shirko.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _service;
        private readonly ICategoryService _categoryService;

        public ProductController(
            IProductService service,
            ICategoryService categoryService)
        {
            _service = service;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(
            string? categoryNormalizedName,
            int pageNo = 1)
        {
            var responseData =
                await _service.GetProductListAsync(
                    categoryNormalizedName,
                    pageNo);

            if (!responseData.Successfull)
                return NotFound(responseData.ErrorMessage);

            var categoryResponse =
                await _categoryService.GetCategoryListAsync();

            if (!categoryResponse.Successfull)
                return NotFound(categoryResponse.ErrorMessage);

            ViewBag.Categories = categoryResponse.Data;
            ViewBag.CurrentCategory = categoryNormalizedName;

            return View(responseData.Data);
        }
    }
}