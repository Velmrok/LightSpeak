using Common.Dto;

namespace ProfileService.src.services;

public interface IServersProfileService
{
    Task<CallResult> AddNewUserServerPositionAsync(string userId, string serverId,CancellationToken cancellationToken);
}