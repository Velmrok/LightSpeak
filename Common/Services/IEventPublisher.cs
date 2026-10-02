using Common.Dto.Events;

namespace Common.Services;

public interface IEventPublisher
{
    Task PublishEventAsync<TEvent>(TEvent message , CancellationToken ct=default) where TEvent : IEvent;
}