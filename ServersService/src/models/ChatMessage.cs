namespace ServersService.src.models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string SenderId { get; set; }
    public required string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public required string ChannelId { get; set; }
    public Channel Channel { get; set; } = null!;
}