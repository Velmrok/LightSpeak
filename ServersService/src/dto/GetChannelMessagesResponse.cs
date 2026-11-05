namespace ServersService.src.dto;

public record GetChannelMessagesResponse(
    List<ChannelMessage> Messages
);