namespace Common.Dto.Events;

public interface IEvent
{
    static abstract string RoutingKey { get; }
}