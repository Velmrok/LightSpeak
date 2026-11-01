using Common.Constants;
using Common.Grpc;
using Protos;
using Common.Mappers;
using Common.Dto;
namespace Common.Clients;

public class ServersGrpcClient(ServersService.ServersServiceClient client, GrpcCallHandler handler) : IServersClient
{
    public string Section => ResourcesSectionNames.Servers;

    public Task<CallResult<Dto.GetServersResponse>> GetServersAsync(string userId, CancellationToken cancellationToken)
    {
        var request = new GetServersRequest { UserId = userId };

        var result = handler.SafeCall(
            Section,
            async (deadline, ct) =>
            {
                return (await client.GetServersAsync(request, deadline: deadline, cancellationToken: ct)).MapToDomainDto();
            }, cancellationToken);

        return result;
    }


} 