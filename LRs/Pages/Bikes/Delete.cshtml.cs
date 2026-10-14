using LRs.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class DeleteModel : PageModel
{
    private readonly IProductService _service;

    public DeleteModel(IProductService service)
    {
        _service = service;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        var bikeId = id ?? Bike.Id;

        if (bikeId <= 0)
            return NotFound();

        try
        {
            await _service.DeleteProductAsync(bikeId);
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var response = await _service.GetProductByIdAsync(bikeId);

            if (!response.Successfull || response.Data is null)
                return NotFound();

            Bike = response.Data;

            return Page();
        }

        return RedirectToPage("./Index");
    }
}