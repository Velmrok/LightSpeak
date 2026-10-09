using Common.Dto;
using ServersService.src.permissions;

namespace ServersService.src.services;
public interface IPermissionService
{
    Task<CallResult<Permission>> GetPermissionsByServerIdAsync(string userId, string serverId, CancellationToken ct);
    Task<CallResult<Permission>> GetPermissionsByChannelIdAsync(string userId, string channelId, CancellationToken ct);
    Task<CallResult<List<string>>> FilterServerMembersWithPermissionAsync(string serverId, Permission required, CancellationToken ct);
    Task<CallResult<List<string>>> FilterChannelMembersWithPermissionAsync(string channelId, Permission required, CancellationToken ct);
}