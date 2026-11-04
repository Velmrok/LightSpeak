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
using Common.Errors;
namespace ProfileService.src.grpc;

public class ProfileGrpcService(AppDbContext appDbContext, IProfileApplicationService profileService) : Protos.ProfileService.ProfileServiceBase
{

    [Authorize]
    public override async Task<Protos.GetUserProfileResponse> GetUserProfile(GetUserProfileRequest request, ServerCallContext context)
    {
        string userId = context.GetHttpContext()?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub)!;
        var profile = await appDbContext.Profiles.Include(p => p.ServerPositions).FirstOrDefaultAsync(p => p.Id == userId, cancellationToken: CancellationToken.None);
        if (profile == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Profile.NotFound", "Profile not found.");
            throw error.ToRpcException();
        }
        var orderedServerIds = profile.ServerPositions.OrderBy(x => x.Position).Select(x => x.ServerId).ToList();

        var response = new Protos.GetUserProfileResponse
        {
            UserId = profile.Id,
            Username = profile.Username,
            Email = profile.Email,
            AvatarUrl = profile.AvatarUrl
        };
        response.ServerPositions.AddRange(orderedServerIds);
        return response;
    }
    [Authorize]
    public override async Task<Google.Protobuf.WellKnownTypes.Empty> GetAuthCheck(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
    {
        return new Google.Protobuf.WellKnownTypes.Empty();
    }
    public override async Task<Protos.GetUserSnapshotResponse> GetUserSnapshot(GetUserSnapshotRequest request, ServerCallContext context)
    {
        var result = await profileService.GetUserSnapshotAsync(request.UserId, CancellationToken.None);
        if (!result.IsSuccess)
        {
            throw result.Error!.ToRpcException();
        }
        var snapshot = result.Data!;

        return snapshot.MapToGrpcDto();
    }
}
