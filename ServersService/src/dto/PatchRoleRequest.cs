using ServersService.src.permissions;

namespace ServersService.src.dto;

public record PatchRoleRequest(string RoleId, string? NewName,  List<Permission>? AddedPermissions, List<Permission>? RemovedPermissions);