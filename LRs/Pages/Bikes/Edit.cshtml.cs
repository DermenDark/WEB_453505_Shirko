using LRs.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class EditModel : PageModel
{
    private readonly IProductService _service;

    public EditModel(IProductService service)
    {
        _service = service;
    }

    [BindProperty]
    public Bike Bike { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
            return NotFound();

        var response = await _service.GetProductByIdAsync(id.Value);

        if (!response.Successfull || response.Data is null)
            return NotFound();

        Bike = response.Data;

        await LoadCategoriesAsync(Bike.CategoryId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(Bike.CategoryId);
            return Page();
        }

        try
        {
            await _service.UpdateProductAsync(Bike.Id, Bike, Image);
        }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            await LoadCategoriesAsync(Bike.CategoryId);
            return Page();
        }

        return RedirectToPage("./Index");
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