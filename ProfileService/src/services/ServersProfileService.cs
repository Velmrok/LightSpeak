using Common.Constants;
using Common.Dto;
using ProfileService.src.database;
using Microsoft.EntityFrameworkCore;

namespace ProfileService.src.services;

public class ServersProfileService(AppDbContext DbContext) : IServersProfileService
{
    public string Section => ResourcesSectionNames.Profile; 
    public async Task<CallResult> AddNewUserServerPositionAsync(string userId, string serverId, CancellationToken cancellationToken)
    {
        var alreadyExists = await DbContext.UserServerPositions
            .AnyAsync(p => p.UserId == userId && p.ServerId == serverId, cancellationToken);

        if (alreadyExists)
            return CallResult.Success(Section);

        var min = await DbContext.UserServerPositions
            .Where(p => p.UserId == userId)
            .MinAsync(p => (int?)p.Position, cancellationToken) ?? 0;

        DbContext.UserServerPositions.Add(new UserServerPosition
        {
            UserId = userId,
            ServerId = serverId,
            Position = min - 1
        });

        await DbContext.SaveChangesAsync(cancellationToken);

        return CallResult.Success(Section);
    }


}