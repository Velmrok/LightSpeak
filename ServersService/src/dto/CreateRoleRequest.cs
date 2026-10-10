using ServersService.src.permissions;

namespace ServersService.src.dto;
public record CreateRoleRequest(string Name,int Priority, List<Permission> Permissions);