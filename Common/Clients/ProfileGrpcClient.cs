using Common.Constants;
using Common.Grpc;
using Protos;
using Common.Mappers;
namespace Common.Clients;

public class ProfileGrpcClient(ProfileService.ProfileServiceClient client, GrpcCallHandler handler) : IProfileClient
{

    public async Task<CallResult<Dto.GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken ct)
    {
       var request = new GetUserSnapshotRequest { UserId = userId };

       var result = await handler.SafeCall(
        async (deadline, ct) =>
        {
            return (await client.GetUserSnapshotAsync(request,deadline: deadline, cancellationToken: ct)).MapToDomainDto();
            
        }, ct);

        
       
       return result;
    
    }
} 