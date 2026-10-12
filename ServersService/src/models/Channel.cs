namespace ServersService.src.models;

public abstract class Channel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<ChatMessage> Messages { get; set; } = [];
}