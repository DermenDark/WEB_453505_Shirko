using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LRs.ApiService;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class IndexModel : PageModel
{
    private readonly IProductService _service;

    public IndexModel(IProductService service)
    {
        _service = service;
    }

    public IList<Bike> Bike { get; set; } = new List<Bike>();

    public async Task OnGetAsync()
    {
        var response = await _service.GetProductListAsync(
            categoryNormalizedName: null,
            pageNo: 1);

        if (!response.Successfull || response.Data is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Ќе удалось получить список велосипедов из API.");

            return;
        }

        var allBikes = response.Data.Items.ToList();

        for (int page = 2; page <= response.Data.TotalPages; page++)
        {
            var nextResponse = await _service.GetProductListAsync(
                categoryNormalizedName: null,
                pageNo: page);

            if (!nextResponse.Successfull || nextResponse.Data is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"Ќе удалось загрузить страницу {page}.");

                break;
            }

            allBikes.AddRange(nextResponse.Data.Items);
        }

        Bike = allBikes;
    }
}
