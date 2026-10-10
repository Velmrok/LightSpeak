using Common.Dto;
using ServersService.src.dto;

namespace   ServersService.src.services;
public interface IRoleService
{
   Task<CallResult<GetRolesResponse>> GetRolesAsync(string serverId,string userId, CancellationToken ct);
   Task<CallResult<CreateRoleResponse>> CreateRoleAsync(string serverId,string userId, CreateRoleRequest request, CancellationToken ct);
    Task<CallResult<PatchRoleResponse>> PatchRoleAsync(string serverId,string userId, PatchRoleRequest request, CancellationToken ct);
   // Task<CallResult> DeleteRoleAsync(string serverId, string roleId, CancellationToken ct);
}