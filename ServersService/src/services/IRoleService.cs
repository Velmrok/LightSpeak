using Common.Dto;
using ServersService.src.dto;

namespace   ServersService.src.services;
public interface IRoleService
{
   Task<CallResult<GetRolesResponse>> GetRolesAsync(string serverId,string userId, CancellationToken ct);
   // Task<CallResult> CreateRoleAsync(string serverId, string name, List<Permission> permissions, CancellationToken ct);
    Task<CallResult<PatchRoleResponse>> PatchRoleAsync(string serverId,string userId, PatchRoleRequest request, CancellationToken ct);
   // Task<CallResult> DeleteRoleAsync(string serverId, string roleId, CancellationToken ct);
}