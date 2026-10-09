

using ServersService.src.models;
using ServersService.src.permissions;

namespace LightSpeak.UnitTests.src.servers;

public static class ChannelExtensions
{
    public static Channel AddRoleOverwrite(this Channel channel, string roleId, Permission allow, Permission deny)
    {
        channel.PermissionOverwrites.Add(CreateOverwrite(channel.Id, roleId, OverwriteTargetType.Role, allow, deny));
        return channel;
    } 
    public static Channel AddMemberOverwrite(this Channel channel, string memberId, Permission allow, Permission deny)
    {
        channel.PermissionOverwrites.Add(CreateOverwrite(channel.Id, memberId, OverwriteTargetType.Member, allow, deny));
        return channel;
    }
    private static ChannelPermissionOverwrite CreateOverwrite(string channelId, string targetId, OverwriteTargetType targetType, Permission allow, Permission deny)
    {
        return new ChannelPermissionOverwrite
        {
            ChannelId = channelId,
            TargetId = targetId,
            TargetType = targetType,
            Allow = allow,
            Deny = deny
        };
    } 
    
}