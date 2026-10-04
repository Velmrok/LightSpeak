namespace Common.Dto.Events;

public record MessageCreatedEvent(string MessageId, string ChannelId, SenderSnapshot Sender, string Content, List<string> RecipientUserIds, DateTime CreatedAt) : IEvent
{
    public static string RoutingKey => "message.created";
}
public record SenderSnapshot(string UserId, string Username, string? AvatarUrl);

