using ServersService.src.permissions;

namespace ServersService.src.models;

public class Role
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; }
    public required Permission Permissions { get; set; }
    public required string ServerId { get; set; } 
    public required int Priority { get; set; }
}