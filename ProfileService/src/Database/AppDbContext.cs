using Microsoft.EntityFrameworkCore;

namespace ProfileService.src.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


}