using Common.Constants;
using Common.Dto;
using Common.Errors;
using ProfileService.src.database;

namespace ProfileService.src.services;

public class ProfileApplicationService(AppDbContext appDbContext) : IProfileApplicationService
{
    public string Section => ResourcesSectionNames.Profile;
    public async Task<CallResult<Empty>> CreateProfileAsync(Profile profile, CancellationToken ct)
    {
        var existingProfile = await appDbContext.Profiles.FindAsync([profile.Id], cancellationToken: ct);
        if (existingProfile != null)
        {
            var error = new AppError(DomainErrorCode.Conflict, "Profile.AlreadyExists", "Profile already exists.");
            return CallResult<Empty>.Fail(Section,error);
        }

        appDbContext.Profiles.Add(profile);
        await appDbContext.SaveChangesAsync(ct);

        return CallResult<Empty>.Success(Section);
    }

    public async Task<CallResult<Profile>> GetProfileAsync(string userId, CancellationToken ct)
    {
        var profile = await appDbContext.Profiles.FindAsync(userId);
        if (profile == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Profile.NotFound", "Profile not found.");
            return CallResult<Profile>.Fail(Section,error);
        }
        return CallResult<Profile>.Success(Section, profile);
    }

    public async Task<CallResult<GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken ct)
    {
        var profile = appDbContext.Profiles.Find(userId);
        if (profile == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Profile.NotFound", "Profile not found.");
            return CallResult<GetUserSnapshotResponse>.Fail(Section, error);
        }

        var response = new GetUserSnapshotResponse
        (
            UserId: profile.Id,
            Username: profile.Username,
            AvatarUrl: "placeholder" // Replace with actual profile picture URL
        );
        return CallResult<GetUserSnapshotResponse>.Success(Section, response);
    }
}
    
