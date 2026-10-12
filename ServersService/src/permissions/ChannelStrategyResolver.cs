using ServersService.src.models;

namespace ServersService.src.permissions;
public interface IChannelStrategyResolver
{
    IChannelStrategy For(Channel channel);
}

public class ChannelStrategyResolver(IEnumerable<IChannelStrategy> strategies) : IChannelStrategyResolver
{
    private readonly Dictionary<Type, IChannelStrategy> _map = strategies.ToDictionary(s => s.ChannelType);

    public IChannelStrategy For(Channel channel)
        => _map.TryGetValue(channel.GetType(), out var s)
            ? s
            : throw new InvalidOperationException($"No strategy for {channel.GetType().Name}");
}