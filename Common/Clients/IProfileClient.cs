using Common.Dto;
using Common.Grpc;



namespace Common.Clients;

public interface IProfileClient : ISectionProvider
{
    Task<CallResult<GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken cancellationToken);
    Task<CallResult<GetUserProfileResponse>> GetUserProfileAsync(string userId, CancellationToken cancellationToken);
}

