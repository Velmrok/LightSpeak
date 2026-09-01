namespace ServersService.src.models;

public class Server
{
    public required string Id { get; set; }
    public required string Name { get; set; } 
    public List<Channel> Channels { get; set; } = [];
    public List<MemberSnapshot> Members { get; set; } = [];
}