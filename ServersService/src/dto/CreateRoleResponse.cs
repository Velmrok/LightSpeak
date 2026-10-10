using ServersService.src.permissions;

namespace ServersService.src.dto;
public record CreateRoleResponse(string Id, string Name, int Priority, List<Permission> Permissions);