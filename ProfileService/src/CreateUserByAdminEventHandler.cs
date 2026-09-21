using Common.Dto;
using MassTransit;
using ProfileService.src.database;
using ProfileService.src.services;

namespace ProfileService.src;

public class CreateUserByAdminEventHandler
{
    private readonly IProfileApplicationService _profileService;
    public CreateUserByAdminEventHandler(IProfileApplicationService profileService)
    {
        _profileService = profileService;
    }
   
    public async Task Handle(KeycloakAdminCreateEvent evt)
    {
        var representation = System.Text.Json.JsonSerializer.Deserialize<KeycloakAdminCreateEventRepresentation>(evt.Representation);
        ArgumentNullException.ThrowIfNull(representation, "Failed to deserialize KeycloakAdminCreateEventRepresentation");

        var id = evt.ResourcePath.Split('/').LastOrDefault();
        ArgumentNullException.ThrowIfNull(id, "Failed to extract user id from ResourcePath");

        ArgumentException.ThrowIfNullOrEmpty(representation.Username, $"XDDDDDDDDDDDDD {evt}");

        var profile = new Profile
        {
            Id = id,
            Username = representation.Username,
            Email = representation.Email,
            CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(evt.Time).UtcDateTime
        };
        var result = await _profileService.CreateProfileAsync(profile, CancellationToken.None);

        if (!result.IsSuccess())
            return; // LOGGING
        

        
    }
}