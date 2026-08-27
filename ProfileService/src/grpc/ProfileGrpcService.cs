using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ProfileService.src.database;
using Protos;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Common.Grpc;
using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using ProfileService.src.services;
using ProfileService.src.extensions;
namespace ProfileService.src.grpc;

public class ProfileGrpcService : Protos.ProfileService.ProfileServiceBase
{   
    private readonly IProfileApplicationService _profileService;
    public ProfileGrpcService(IProfileApplicationService profileService)
    {
        _profileService = profileService;
    }
    [Authorize]
    public override async Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        string userId = context.GetHttpContext()?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var result = await _profileService.GetProfileAsync(userId, CancellationToken.None);
        if (result.IsError)
        {
            throw result.FirstError.ToRpcException();
        }
        var profile = result.Value;

        var response = new GetProfileResponse
        {
            UserId = profile.Id,
            Username = profile.Username,
            Email = profile.Email
        };
        return await Task.FromResult(response);
    }
    [Authorize]
    public override async Task<Empty> GetAuthCheck(Empty request, ServerCallContext context)
    {
        return await Task.FromResult(new Empty());
    }
}
