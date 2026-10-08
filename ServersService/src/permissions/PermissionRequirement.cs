using Microsoft.AspNetCore.Authorization;

namespace ServersService.src.permissions;

public sealed class PermissionRequirement(Permission permission) : IAuthorizationRequirement
{
    public Permission Permission { get; } = permission;
}