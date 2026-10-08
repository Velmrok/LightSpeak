namespace ServersService.src.models;

public class Channel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public List<ChatMessage> Messages { get; set; } = [];
    public required string ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public List<ChannelPermissionOverwrite> PermissionOverwrites { get; set; } = [];
}