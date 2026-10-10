using ServersService.src.models;

namespace ServersService.src.permissions;

public static class PermissionCalculator
{
     public static  Permission CalculatePermissions(Member? member)
    {
        Permission perms = Permission.None;
        if (member != null)
        {
            foreach (var role in member.Roles)
            {
                perms |= role.Permissions;
            }
        }
        return perms;
    }
}