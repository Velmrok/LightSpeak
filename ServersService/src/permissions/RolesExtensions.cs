using ServersService.src.models;

namespace ServersService.src.permissions;

public static class RolesExtensions
{
    public static Role? GetMostPowerfulRole(this IEnumerable<Role> roles)
    {
        return roles.OrderByDescending(r => r.Permissions).FirstOrDefault();
    }
}