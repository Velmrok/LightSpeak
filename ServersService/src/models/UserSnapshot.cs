namespace ServersService.src.models;
public class UserSnapshot
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string AvatarUrl { get; set; }
    public List<Member> Memberships { get; set; } = [];
}