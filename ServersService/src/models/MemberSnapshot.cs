namespace ServersService.src.models;

public class MemberSnapshot
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string AvatarUrl { get; set; }
    public required string ServerId { get; set; }
    public required Server Server { get; set; }
}