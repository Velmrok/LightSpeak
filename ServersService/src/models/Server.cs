namespace ServersService.src.models;

public class Server
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; } 
    public List<Channel> Channels { get; set; } = [];
    public List<Member> Members { get; set; } = [];
    public List<Role> Roles { get; set; } = [];
}