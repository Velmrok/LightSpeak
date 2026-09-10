using Common.Dto;
using ErrorOr;
using ProfileService.src.database;

namespace ProfileService.src.services;

public class ProfileApplicationService(AppDbContext appDbContext) : IProfileApplicationService
{
    public async Task<ErrorOr<Success>> CreateProfileAsync(Profile profile, CancellationToken ct)
    {
        var existingProfile = await appDbContext.Profiles.FindAsync([profile.Id], cancellationToken: ct);
        if (existingProfile != null) return Error.Conflict("Profile.AlreadyExists", "Profile already exists.");

        appDbContext.Profiles.Add(profile);
        await appDbContext.SaveChangesAsync(ct);

        return Result.Success;
    }

    public async Task<ErrorOr<Profile>> GetProfileAsync(string userId, CancellationToken ct)
    {
        var profile = await appDbContext.Profiles.FindAsync(userId);
        if (profile == null) return Error.NotFound("Profile.NotFound", "Profile not found.");
        return profile;
    }

    public async Task<ErrorOr<GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken ct)
    {
        var profile = appDbContext.Profiles.Find(userId);
        if (profile == null) return Error.NotFound("Profile.NotFound", "Profile not found.");

        var response = new GetUserSnapshotResponse
        (
            UserId: profile.Id,
            Username: profile.Username,
            AvatarUrl: "placeholder" // Replace with actual profile picture URL
        );
        return response;
    }
}
    
