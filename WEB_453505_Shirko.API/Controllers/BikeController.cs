using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEB_453505_Shirko.API.Data;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.API.Controllers;

[Route("api/[controller]/mvc")]
[ApiController]
public class BikeController : ControllerBase
{
    private readonly AppDbContext _context;

    public BikeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bike>>> GetBikes()
    {
        return await _context.Bikes
            .Include(b => b.Category)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Bike>> GetBike(int id)
    {
        var bike = await _context.Bikes
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bike == null)
            return NotFound();

        return bike;
    }
}