using ServersService.src.models;

namespace ServersService.src.permissions;
public interface IChannelStrategy
{
    Type ChannelType { get; }
    Task<bool> IsMemberAsync(string userId, Channel channel, CancellationToken ct);
    Task<bool> CanWriteAsync(string userId, Channel channel, CancellationToken ct);
    Task<IReadOnlyList<string>> GetRecipientsAsync(Channel channel, CancellationToken ct);
}