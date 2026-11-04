using Common.Constants;
using Common.Dto;
using ProfileService.src.database;

namespace ProfileService.src.services;

public class ServersProfileService(AppDbContext DbContext) : IServersProfileService
{
    public string Section => ResourcesSectionNames.Profile;
    public async Task<CallResult> AddNewUserServerPositionAsync(string userId, string serverId, CancellationToken cancellationToken)
    {
        var min = DbContext.UserServerPositions
            .Where(p => p.UserId == userId)
            .Select(p => p.Position)
            .DefaultIfEmpty(0)
            .Min();
        var newPosition = min - 1;
        var newUserServerPosition = new UserServerPosition
        {
            UserId = userId,
            ServerId = serverId,
            Position = newPosition
        };
        
        DbContext.UserServerPositions.Add(newUserServerPosition);
        await DbContext.SaveChangesAsync(cancellationToken);
        return CallResult.Success(Section);
    }


}