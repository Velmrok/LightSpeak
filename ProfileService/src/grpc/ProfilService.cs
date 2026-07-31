using Grpc.Core;
using Protos;

namespace ProfileService.src.grpc;

public class ProfileGrpcService : Protos.ProfileService.ProfileServiceBase
{   
    public override async Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        var response = new GetProfileResponse
        {
            UserId = request.UserId,
            Name = "John Doe",
            Email = "john.doe@example.com"
        };
        return await Task.FromResult(response);
    }
}
