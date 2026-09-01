namespace ServersService.src.models;

public class Channel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];
    public required string ServerId { get; set; }
    public required Server Server { get; set; }
}