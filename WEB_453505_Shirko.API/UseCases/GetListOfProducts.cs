using MediatR;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace WEB_453505_Shirko.API.UseCases;

public sealed record GetListOfProducts(
    string? categoryNormalizedName,
    int pageNo = 1,
    int pageSize = 3
) : IRequest<ResponseData<ListModel<Bike>>>;

public class GetListOfProductsHandler(AppDbContext db)
    : IRequestHandler<GetListOfProducts, ResponseData<ListModel<Bike>>>
{
    // Максимальный размер страницы
    private readonly int _maxPageSize = 20;

    public async Task<ResponseData<ListModel<Bike>>> Handle(
        GetListOfProducts request,
        CancellationToken cancellationToken)
    {
        var result = new ListModel<Bike>();

        int pageSize = request.pageSize;

        if (pageSize > _maxPageSize)
            pageSize = _maxPageSize;

        if (pageSize < 1)
            pageSize = 1;
        var query = db.Bikes
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.categoryNormalizedName))
        {
            query = query.Where(b =>
                b.Category != null &&
                b.Category.NormalizedName ==
                request.categoryNormalizedName);
        }

        int totalItems = await query.CountAsync(cancellationToken);

        result.TotalPages = (int)Math.Ceiling(
            totalItems / (double)pageSize);

        if (result.TotalPages == 0)
            result.TotalPages = 1;

        int pageNo = request.pageNo;

        if (pageNo < 1)
            pageNo = 1;

        if (pageNo > result.TotalPages)
            pageNo = result.TotalPages;

        result.CurrentPage = pageNo;
        result.Items = await query
            .Skip((pageNo - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ResponseData<ListModel<Bike>>
        {
            Data = result,
            Successfull = true
        };
    }
}