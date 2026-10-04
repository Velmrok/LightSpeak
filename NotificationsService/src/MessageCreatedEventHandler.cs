using Common.Dto.Events;
using Microsoft.AspNetCore.SignalR;
using NotificationsService.src.dto;

namespace NotificationsService.src;

public class MessageCreatedEventHandler
{
    private readonly IHubContext<AppHub, IAppHubClient> _hubContext;

    public MessageCreatedEventHandler(IHubContext<AppHub, IAppHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Handle(MessageCreatedEvent messageCreatedEvent)
    {
        var ids = messageCreatedEvent.RecipientUserIds;
        var clientDto = new MessageCreatedClientDto(
            messageCreatedEvent.MessageId,
            messageCreatedEvent.ChannelId,
            messageCreatedEvent.Content,
            messageCreatedEvent.CreatedAt,
            messageCreatedEvent.Sender
        );
        await _hubContext.Clients.Users(ids).MessageCreated(clientDto);
    }
}