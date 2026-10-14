using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WEB_453505_Shirko.API.Migrations;
using WEB_453505_Shirko.Domain.Entities;

namespace WEB_453505_Shirko.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Bike> Bikes { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
