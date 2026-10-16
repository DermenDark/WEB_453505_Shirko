
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.API.UseCases;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;
using Xunit;

namespace WEB_453505_Shirko.Tests.API;

public class GetListOfProductsHandlerTests
{
    [Fact]
    public async Task Handle_DefaultRequest_ReturnsFirstPageOfThreeBikes()
    {
        await using var database = await CreateDatabaseAsync();
        var handler = new GetListOfProductsHandler(database.Context);

        var response = await handler.Handle(
            new GetListOfProducts(null),
            CancellationToken.None);

        Assert.True(response.Successfull);
        Assert.NotNull(response.Data);

        Assert.Equal(1, response.Data.CurrentPage);
        Assert.Equal(3, response.Data.Items.Count);
        Assert.Equal(9, response.Data.TotalPages);

        Assert.Equal(
            new[] { 1, 2, 3 },
            response.Data.Items.Select(b => b.Id).ToArray());
    }

    [Fact]
    public async Task Handle_SecondPage_ReturnsNextThreeBikes()
    {
        await using var database = await CreateDatabaseAsync();
        var handler = new GetListOfProductsHandler(database.Context);

        var response = await handler.Handle(
            new GetListOfProducts(null, pageNo: 2),
            CancellationToken.None);

        Assert.True(response.Successfull);
        Assert.NotNull(response.Data);

        Assert.Equal(2, response.Data.CurrentPage);
        Assert.Equal(3, response.Data.Items.Count);

        Assert.Equal(
            new[] { 4, 5, 6 },
            response.Data.Items.Select(b => b.Id).ToArray());
    }

    [Fact]
    public async Task Handle_RoadCategory_ReturnsOnlyRoadBikes()
    {
        await using var database = await CreateDatabaseAsync();
        var handler = new GetListOfProductsHandler(database.Context);

        var response = await handler.Handle(
            new GetListOfProducts(
                categoryNormalizedName: "road",
                pageNo: 1,
                pageSize: 20),
            CancellationToken.None);

        Assert.True(response.Successfull);
        Assert.NotNull(response.Data);

        Assert.Equal(15, response.Data.Items.Count);
        Assert.Equal(1, response.Data.TotalPages);

        Assert.All(
            response.Data.Items,
            bike => Assert.Equal(1, bike.CategoryId));
    }

    [Fact]
    public async Task Handle_PageSizeAboveMaximum_LimitsPageToTwentyBikes()
    {
        await using var database = await CreateDatabaseAsync();
        var handler = new GetListOfProductsHandler(database.Context);

        var response = await handler.Handle(
            new GetListOfProducts(
                categoryNormalizedName: null,
                pageNo: 1,
                pageSize: 100),
            CancellationToken.None);

        Assert.True(response.Successfull);
        Assert.NotNull(response.Data);

        Assert.Equal(20, response.Data.Items.Count);
        Assert.Equal(2, response.Data.TotalPages);
    }

    [Fact]
    public async Task Handle_PageNumberAboveTotalPages_ReturnsFailure()
    {
        await using var database = await CreateDatabaseAsync();
        var handler = new GetListOfProductsHandler(database.Context);

        var response = await handler.Handle(
            new GetListOfProducts(
                categoryNormalizedName: null,
                pageNo: 10,
                pageSize: 3),
            CancellationToken.None);

        Assert.False(response.Successfull);
        Assert.NotNull(response.ErrorMessage);
    }

    private static async Task<TestDatabase> CreateDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        await SeedDatabaseAsync(context);

        return new TestDatabase(connection, context);
    }

    private static async Task SeedDatabaseAsync(AppDbContext context)
    {
        var road = new Category
        {
            Id = 1,
            Name = "Road",
            NormalizedName = "road"
        };

        var gravel = new Category
        {
            Id = 2,
            Name = "Gravel",
            NormalizedName = "gravel"
        };

        context.Categories.AddRange(road, gravel);
        await context.SaveChangesAsync();

        var bikes = Enumerable.Range(1, 25)
            .Select(id => new Bike
            {
                Id = id,
                Name = $"Test Bike {id}",
                Description = $"Test description {id}",
                CategoryId = id <= 15 ? road.Id : gravel.Id,
                Price = 100 + id
            });

        context.Bikes.AddRange(bikes);
        await context.SaveChangesAsync();
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        public SqliteConnection Connection { get; }
        public AppDbContext Context { get; }

        public TestDatabase(
            SqliteConnection connection,
            AppDbContext context)
        {
            Connection = connection;
            Context = context;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
}
