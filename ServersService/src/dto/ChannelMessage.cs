namespace ServersService.src.dto;

public record ChannelMessage(
    string Id,
    string ChannelId,
    string SenderId,
    string Content,
    DateTime Timestamp
);