using Common.Dto;
using MassTransit;
using ProfileService.src.database;

namespace ProfileService.src;

public class RegisterEventHandler
{
    private readonly AppDbContext _dbContext;

    public RegisterEventHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Handle(KeycloakRegisterEvent evt)
    {
        
        var existingProfile = await _dbContext.Profiles.FindAsync(evt.UserId);
        if (existingProfile != null) return; 

        var profile = new Profile
        {
            Id = evt.UserId,
            Name = evt.Details.Username,
            Email = evt.Details.Email,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(evt.Time).UtcDateTime
        };

        _dbContext.Profiles.Add(profile);
        await _dbContext.SaveChangesAsync();
    }
}