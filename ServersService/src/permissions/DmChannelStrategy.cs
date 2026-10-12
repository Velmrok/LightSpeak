using ServersService.src.models;

namespace ServersService.src.permissions;
public class DmChannelStrategy : IChannelStrategy
{
    public Type ChannelType => typeof(DmChannel);

    public Task<bool> IsMemberAsync(string userId, Channel channel, CancellationToken ct)
    {
        var dm = (DmChannel)channel;
        return Task.FromResult(dm.UserLowId == userId || dm.UserHighId == userId);
    }

    public Task<bool> CanWriteAsync(string userId, Channel channel, CancellationToken ct)
        => IsMemberAsync(userId, channel, ct);  

    public Task<IReadOnlyList<string>> GetRecipientsAsync(Channel channel, CancellationToken ct)
    {
        var dm = (DmChannel)channel;
        return Task.FromResult<IReadOnlyList<string>>([dm.UserLowId, dm.UserHighId]);
    }
}

