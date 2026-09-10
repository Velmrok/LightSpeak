using ErrorOr;
using ServersService.src.dto;

namespace ServersService.src.services;

public interface IServersApplicationService
{
    Task<ErrorOr<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken);
}