namespace ServersService.src.models;
public class ServerChannel: Channel
{
    public required string Name { get; set; }
    public required string ServerId { get; set; }
    public Server Server { get; set; } = null!;
    public List<ChannelPermissionOverwrite> PermissionOverwrites { get; set; } = [];
}