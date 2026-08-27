using ErrorOr;

namespace ProfileService.src.services;

public interface IProfileApplicationService
{
    Task<ErrorOr<Profile>> GetProfileAsync(string userId, CancellationToken ct);
    Task<ErrorOr<Success>> CreateProfileAsync(Profile profile, CancellationToken ct);
}