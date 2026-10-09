using ServersService.src.models;
using ServersService.src.permissions;

namespace ServersService.src;

public static class DefaultRoles
{
    public const Permission EveryonePermissions = Permission.ReadOnChannel | Permission.WriteOnChannel;
    public static Role CreateEveryone(string serverId) => new()
    {
        Name = "Everyone",
        Permissions = EveryonePermissions,
        ServerId = serverId,
        Priority = 1
    };
    public static Role CreateOwner(string serverId) => new()
    {
        Name = "Owner",
        Permissions = Permission.All,
        ServerId = serverId,
        Priority = 0
    };
}