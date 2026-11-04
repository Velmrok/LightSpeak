using Microsoft.EntityFrameworkCore;

namespace ProfileService.src.database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<UserServerPosition> UserServerPositions => Set<UserServerPosition>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedNever();
        });
        modelBuilder.Entity<UserServerPosition>(e =>
        {
            e.HasKey(p => new { p.UserId, p.ServerId });

            e.HasOne(p => p.Profile)
            .WithMany(pr => pr.ServerPositions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => new { p.UserId, p.Position }).IsUnique();
        });
    }
}