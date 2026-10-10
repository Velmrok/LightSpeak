using ServersService.src.permissions;

namespace ServersService.src.dto;

public record GetRolesResponse(List<RoleDto> Roles);
public record RoleDto(string RoleId, string Name, List<Permission> Permissions);