using Common.Dto;

using ServersService.src.dto;

namespace ServersService.src.services;

public interface IServersApplicationService
{
    Task<CallResult<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken);
}