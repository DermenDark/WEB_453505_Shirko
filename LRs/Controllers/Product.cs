using System;
using LRs.ApiService;
using LRs.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [AllowAnonymous]
        [HttpGet("/Catalog/{categoryNormalizedName?}")]
        public async Task<IActionResult> Index(
            string? categoryNormalizedName,
            int pageNo = 1)
        {
            var responseData =
                await _service.GetProductListAsync(
                    categoryNormalizedName,
                    pageNo);

            if (!responseData.Successfull || responseData.Data is null)
                return NotFound(responseData.ErrorMessage);

            ViewBag.CurrentCategory = categoryNormalizedName;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ListPartial", responseData.Data);
            }

            var categoryResponse =
                await _categoryService.GetCategoryListAsync();

            if (!categoryResponse.Successfull)
                return NotFound(categoryResponse.ErrorMessage);

            ViewBag.Categories = categoryResponse.Data;
            return View(responseData.Data);
        }

    }
}