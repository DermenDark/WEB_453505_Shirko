
using System.Collections.Generic;
using System.Threading.Tasks;
using LRs.ApiService;
using LRs.Services.BikeCategoryService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WEB_453505_Shirko.Controllers;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;
using Xunit;

namespace WEB_453505_Shirko.Tests.Controllers;

public class ProductControllerTests
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        // Arrange: создаём подмены зависимостей.
        _productService = Substitute.For<IProductService>();
        _categoryService = Substitute.For<ICategoryService>();

        _controller = new ProductController(
            _productService,
            _categoryService);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenProductsRequestFails()
    {
        // Arrange
        _productService
            .GetProductListAsync("road", 1)
            .Returns(Task.FromResult(
                new ResponseData<ListModel<Bike>>
                {
                    Successfull = false,
                    ErrorMessage = "Ошибка получения велосипедов"
                }));

        // Act
        var result = await _controller.Index("road", 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(
            "Ошибка получения велосипедов",
            notFound.Value);

        await _categoryService
            .DidNotReceive()
            .GetCategoryListAsync();
    }

    [Fact]
    public async Task Index_ReturnsNotFound_WhenCategoriesRequestFails()
    {
        // Arrange
        _productService
            .GetProductListAsync("road", 1)
            .Returns(Task.FromResult(
                new ResponseData<ListModel<Bike>>
                {
                    Successfull = true,
                    Data = new ListModel<Bike>()
                }));

        _categoryService
            .GetCategoryListAsync()
            .Returns(Task.FromResult(
                new ResponseData<List<Category>>
                {
                    Successfull = false,
                    ErrorMessage = "Ошибка получения категорий"
                }));

        // Act
        var result = await _controller.Index("road", 1);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(
            "Ошибка получения категорий",
            notFound.Value);
    }

    [Fact]
    public async Task Index_ReturnsView_WithProductsAndCategories()
    {
        // Arrange
        var bike = new Bike
        {
            Id = 1,
            Name = "Test Bike"
        };

        var model = new ListModel<Bike>
        {
            Items = new List<Bike> { bike },
            CurrentPage = 1,
            TotalPages = 1
        };

        var categories = new List<Category>
        {
            new Category
            {
                Id = 1,
                Name = "Road",
                NormalizedName = "road"
            }
        };

        _productService
            .GetProductListAsync("road", 1)
            .Returns(Task.FromResult(
                new ResponseData<ListModel<Bike>>
                {
                    Successfull = true,
                    Data = model
                }));

        _categoryService
            .GetCategoryListAsync()
            .Returns(Task.FromResult(
                new ResponseData<List<Category>>
                {
                    Successfull = true,
                    Data = categories
                }));

        // Act
        var result = await _controller.Index("road", 1);

        // Assert
        var view = Assert.IsType<ViewResult>(result);

        Assert.Same(model, view.Model);

        Assert.Equal(
            categories,
            view.ViewData["Categories"]);

        Assert.Equal(
            "road",
            view.ViewData["CurrentCategory"]);
    }

    [Fact]
    public async Task Index_ReturnsPartialView_ForAjaxRequest()
    {
        // Arrange
        var model = new ListModel<Bike>
        {
            Items = new List<Bike>
            {
                new Bike
                {
                    Id = 1,
                    Name = "Test Bike"
                }
            },
            CurrentPage = 1,
            TotalPages = 2
        };

        _productService
            .GetProductListAsync("road", 1)
            .Returns(Task.FromResult(
                new ResponseData<ListModel<Bike>>
                {
                    Successfull = true,
                    Data = model
                }));

        _controller.HttpContext.Request.Headers[
            "X-Requested-With"] = "XMLHttpRequest";

        // Act
        var result = await _controller.Index("road", 1);

        // Assert
        var partial = Assert.IsType<PartialViewResult>(result);

        Assert.Equal("_ListPartial", partial.ViewName);
        Assert.Same(model, partial.Model);

        await _categoryService
            .DidNotReceive()
            .GetCategoryListAsync();
    }
}
