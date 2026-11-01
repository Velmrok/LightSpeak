
using Grpc.Core;
using Protos;
using ServersService.src.services;
using Common.Mappers;

namespace ServersService.src.grpc;

public class ServersGrpcService : Protos.ServersService.ServersServiceBase
{
    private readonly IServersApplicationService _serversService;

    public ServersGrpcService(IServersApplicationService serversService)
    {
        _serversService = serversService;
    }

    public override async Task<GetServersResponse> GetServers(GetServersRequest request, ServerCallContext context)
    {
        var userId = request.UserId;
        var serversResponse = await _serversService.GetServersAsync(userId, context.CancellationToken);
        if (!serversResponse.IsSuccess)
        {
            throw serversResponse.Error!.ToRpcException();
        }
        var servers = serversResponse.Data!.MapToGrpcDto();
        return servers;
    }
}