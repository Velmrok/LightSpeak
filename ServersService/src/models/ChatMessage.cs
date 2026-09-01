namespace ServersService.src.models;

public class ChatMessage
{
    public required string Id { get; set; }
    public required string SenderId { get; set; }
    public required string Content { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string ChannelId { get; set; }
    public required Channel Channel { get; set; }
}