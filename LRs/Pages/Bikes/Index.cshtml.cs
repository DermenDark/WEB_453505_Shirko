
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LRs.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class IndexModel : PageModel
{
    private readonly IProductService _service;

    public IndexModel(IProductService service)
    {
        _service = service;
    }

    [BindProperty(SupportsGet = true)]
    public int PageNo { get; set; } = 1;
    public int TotalPages { get; set; }
    public IList<Bike> Bike { get; set; } = new List<Bike>();

    public async Task OnGetAsync()
    {
        if (PageNo < 1)
        {
            PageNo = 1;
        }

        var response = await _service.GetProductListAsync(
            categoryNormalizedName: null,
            pageNo: PageNo);

        if (!response.Successfull || response.Data is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось получить список велосипедов из API.");

            return;
        }

        TotalPages = response.Data.TotalPages;

        if (TotalPages > 0 && PageNo > TotalPages)
        {
            PageNo = TotalPages;

            response = await _service.GetProductListAsync(
                categoryNormalizedName: null,
                pageNo: PageNo);

            if (!response.Successfull || response.Data is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Не удалось загрузить последнюю страницу.");

                return;
            }

            TotalPages = response.Data.TotalPages;
        }

        Bike = response.Data.Items.ToList();

        var categoriesResponse = await _service.GetCategoriesAsync();

        if (categoriesResponse.Successfull &&
            categoriesResponse.Data is not null)
        {
            var categoriesById = categoriesResponse.Data
                .ToDictionary(category => category.Id);

            foreach (var bike in Bike)
            {
                if (categoriesById.TryGetValue(
                        bike.CategoryId,
                        out var category))
                {
                    bike.Category = category;
                }
            }
        }
    }
    private async Task LoadCategoriesAsync(int? selectedId = null)
    {
        var response = await _service.GetCategoriesAsync();

        var categories =
            response.Successfull && response.Data is not null
                ? response.Data
                : new List<Category>();

        ViewData["CategoryId"] = new SelectList(
            categories, "Id", "Name", selectedId);
    }
}
