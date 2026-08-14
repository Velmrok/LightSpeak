using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ProfileService.src.database;
using Protos;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Common.Grpc;
namespace ProfileService.src.grpc;

public class ProfileGrpcService : Protos.ProfileService.ProfileServiceBase
{   
    private readonly AppDbContext _dbContext;
    public ProfileGrpcService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [Authorize]
    public override async Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
    {
        var userId = context.GetHttpContext()?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var profile = await _dbContext.Profiles.FirstOrDefaultAsync(p => p.Id == userId);
        if (profile == null)
        {
            AppError error = new(StatusCode.NotFound,"profileNotFound", $"Profile not found for userId: {userId}");
            throw error.ToRpcException();
        }
        var response = new GetProfileResponse
        {
            UserId = profile.Id,
            Name = profile.Name,
            Email = profile.Email
        };
        return await Task.FromResult(response);
    }
}
