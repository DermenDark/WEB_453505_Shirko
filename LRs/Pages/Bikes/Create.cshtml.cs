using LRs.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_453505_Shirko.Domain.Entities;

namespace LRs.Pages_Bikes;

public class CreateModel : PageModel
{
    private readonly IProductService _service;

    public CreateModel(IProductService service)
    {
        _service = service;
    }

    [BindProperty]
    public Bike Bike { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadCategoriesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync(Bike.CategoryId);
            return Page();
        }

        var result = await _service.CreateProductAsync(Bike, Image);

        if (!result.Successfull)
        {
            ModelState.AddModelError(
                string.Empty,
                "Не удалось создать велосипед через API.");

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