using Common.Dto.Events;

namespace NotificationsService.src.dto;
public record MessageCreatedClientDto(
    string MessageId,
    string ChannelId,
    string Content,
    DateTime CreatedAt,
    SenderSnapshot Sender);