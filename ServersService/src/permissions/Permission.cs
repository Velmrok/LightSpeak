using System.Text.Json.Serialization;

namespace ServersService.src.permissions;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum Permission : long
{
    None = 0,
    WriteOnChannel = 1L << 0,
    ReadOnChannel = 1L << 1,
    ManageChannels = 1L << 2,
    ManageRoles = 1L << 3,

    UnreachableRolePermission = 1L << 63,
    All = ~0L & ~UnreachableRolePermission
}