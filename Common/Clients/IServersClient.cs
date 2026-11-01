using Common.Dto;
using Common.Grpc;



namespace Common.Clients;

public interface IServersClient : ISectionProvider
{
    Task<CallResult<GetServersResponse>> GetServersAsync(string userId, CancellationToken cancellationToken);
}

