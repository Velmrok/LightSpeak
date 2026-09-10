using Common.Dto;
using Common.Grpc;



namespace Common.Clients;

public interface IProfileClient
{
    Task<CallResult<GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken cancellationToken);
}

