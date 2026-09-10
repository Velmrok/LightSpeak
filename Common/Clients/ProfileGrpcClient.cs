using Common.Constants;
using Common.Grpc;
using Protos;
using Common.Mappers;
using Common.Dto;
namespace Common.Clients;

public class ProfileGrpcClient(ProfileService.ProfileServiceClient client, GrpcCallHandler handler) : IProfileClient
{
    public string Section => ResourcesSectionNames.Profile;

    public Task<CallResult<Dto.GetUserProfileResponse>> GetUserProfileAsync(string userId, CancellationToken cancellationToken)
    {
        var request = new GetUserProfileRequest { UserId = userId };

        var result = handler.SafeCall(
            Section,
            async (deadline, ct) =>
            {
                return (await client.GetUserProfileAsync(request, deadline: deadline, cancellationToken: ct)).MapToDomainDto();
            }, cancellationToken);

        return result;
        
    }

    public async Task<CallResult<Dto.GetUserSnapshotResponse>> GetUserSnapshotAsync(string userId, CancellationToken ct)
    {
       var request = new GetUserSnapshotRequest { UserId = userId };

       var result = await handler.SafeCall(
        Section,
        async (deadline, ct) =>
        {
            return (await client.GetUserSnapshotAsync(request,deadline: deadline, cancellationToken: ct)).MapToDomainDto();
            
        }, ct);

        
       
       return result;
    
    }
} 