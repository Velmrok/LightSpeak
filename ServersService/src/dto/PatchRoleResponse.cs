using ServersService.src.permissions;

namespace ServersService.src.dto;

public record PatchRoleResponse(string RoleId, string Name, List<Permission> Permissions);