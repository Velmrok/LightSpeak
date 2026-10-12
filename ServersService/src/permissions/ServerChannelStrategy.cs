using ServersService.src.models;
using ServersService.src.services;
using Microsoft.EntityFrameworkCore;

namespace ServersService.src.permissions;
public class ServerChannelStrategy(AppDbContext db, IPermissionService permissions) : IChannelStrategy
{
    public Type ChannelType => typeof(ServerChannel);

    public async Task<bool> IsMemberAsync(string userId, Channel channel, CancellationToken ct)
    {
        var sc = (ServerChannel)channel;
        return await db.Members.AnyAsync(m => m.ServerId == sc.ServerId && m.UserId == userId, ct);
    }

    public async Task<bool> CanWriteAsync(string userId, Channel channel, CancellationToken ct)
    {
        var sc = (ServerChannel)channel;
        var perms = await permissions.GetPermissionsByChannelIdAsync(userId, sc.Id, ct);
        if (!perms.IsSuccess) return false;
        return perms.Data.HasFlag(Permission.WriteOnChannel);
    }

    public async Task<IReadOnlyList<string>> GetRecipientsAsync(Channel channel, CancellationToken ct)
    {
        var sc = (ServerChannel)channel;
        var recipientResult = await permissions.FilterChannelMembersWithPermissionAsync(sc.Id, Permission.ReadOnChannel, ct);
        if (!recipientResult.IsSuccess) return [];
        return recipientResult.Data;
    }
}