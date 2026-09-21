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
using Common.Mappers;
using Common.Dto;
namespace ProfileService.src.grpc;

public class ProfileGrpcService : Protos.ProfileService.ProfileServiceBase
{   
    private readonly IProfileApplicationService _profileService;
    public ProfileGrpcService(IProfileApplicationService profileService)
    {
        _profileService = profileService;
    }
    [Authorize]
    public override async Task<Protos.GetUserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        string userId = context.GetHttpContext()?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var result = await _profileService.GetProfileAsync(userId, CancellationToken.None);
        if (!result.IsSuccess())
        {
            throw result.Error!.ToRpcException();
        }
        var profile = result.Data!;

        var response = new Protos.GetUserProfileResponse
        {
            UserId = profile.Id,
            Username = profile.Username,
            Email = profile.Email,
            AvatarUrl = profile.AvatarUrl
        };
        return response;
    }
    [Authorize]
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> GetAuthCheck(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
    {
        return new Google.Protobuf.WellKnownTypes.Empty();
    }
    public override async Task<Protos.GetUserSnapshotResponse> GetUserSnapshot(GetUserSnapshotRequest request, ServerCallContext context)
    {
        var result = await _profileService.GetUserSnapshotAsync(request.UserId, CancellationToken.None);
        if (!result.IsSuccess())
        {
            throw result.Error!.ToRpcException();
        }
        var snapshot = result.Data!;

        return snapshot.MapToGrpcDto();
    }
}
