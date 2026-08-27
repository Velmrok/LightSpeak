using Common.Dto;
using MassTransit;
using ProfileService.src.database;
using ProfileService.src.services;

namespace ProfileService.src;

public class RegisterEventHandler
{
    private readonly IProfileApplicationService _profileService;
    public RegisterEventHandler(IProfileApplicationService profileService)
    {
        _profileService = profileService;
    }
   
    public async Task Handle(KeycloakRegisterEvent evt)
    {
        var profile = new Profile
        {
            Id = evt.UserId,
            Username = evt.Details.Username,
            Email = evt.Details.Email,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(evt.Time).UtcDateTime
        };
        var result = await _profileService.CreateProfileAsync(profile, CancellationToken.None);

        if (result.IsError)
            return; // LOGGING
        

        
    }
}