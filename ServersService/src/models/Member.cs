namespace ServersService.src.models;

public class Member
{
    public required string UserId { get; set; }
    public  UserSnapshot User { get; set; } = null!;
    public required string ServerId { get; set; }
    public Server Server { get; set; } = null!;
}