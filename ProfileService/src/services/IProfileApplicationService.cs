using Common.Clients;
using Common.Dto;

namespace ProfileService.src.services;

public interface IProfileApplicationService : ISectionProvider
{
    Task<CallResult<Profile>> GetProfileAsync(string userId, CancellationToken ct);
    Task<CallResult<Empty>> CreateProfileAsync(Profile profile, CancellationToken ct);
    Task<CallResult<GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken ct);
}