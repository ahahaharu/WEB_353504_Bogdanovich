using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.Domain.Entities;

namespace WEB_353504_Bogdanovich.API.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Category> Categories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
