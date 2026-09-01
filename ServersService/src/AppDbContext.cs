using Microsoft.EntityFrameworkCore;
using ServersService.src.models;

namespace ServersService.src;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Server> Servers => Set<Server>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<MemberSnapshot> MemberSnapshots => Set<MemberSnapshot>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Server>()
            .HasMany(s => s.Channels)
            .WithOne(c => c.Server)
            .HasForeignKey(c => c.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Server>()
            .HasMany(s => s.Members)
            .WithOne(m => m.Server)
            .HasForeignKey(m => m.ServerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Channel>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Channel)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
   
}