using LRs.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class DetailsModel : PageModel
{
    private readonly IProductService _service;

    public DetailsModel(IProductService service)
    {
        _service = service;
    }

    public Bike Bike { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
            return NotFound();

        var response = await _service.GetProductByIdAsync(id.Value);

        if (!response.Successfull || response.Data is null)
            return NotFound();

        Bike = response.Data;

        return Page();
    }
}