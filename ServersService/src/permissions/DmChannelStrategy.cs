using ServersService.src.models;

namespace ServersService.src.permissions;
public class DmChannelStrategy : IChannelStrategy
{
    public Type ChannelType => typeof(DmChannel);

    public async Task<bool> IsMemberAsync(string userId, Channel channel, CancellationToken ct)
    {
        var dm = (DmChannel)channel;
        return userId == dm.UserLowId || userId == dm.UserHighId;
    }

    public async Task<bool> CanWriteAsync(string userId, Channel channel, CancellationToken ct)
        => await IsMemberAsync(userId, channel, ct);  

    public async Task<IReadOnlyList<string>> GetRecipientsAsync(Channel channel, CancellationToken ct)
    {
        var dm = (DmChannel)channel;
        return [dm.UserLowId, dm.UserHighId];
    }

    public async Task<bool> CanReadAsync(string userId, Channel channel, CancellationToken ct)
    {
        return await IsMemberAsync(userId, channel, ct);
    }
}

