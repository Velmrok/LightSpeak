using ServersService.src.permissions;

public class ChannelPermissionOverwrite
{
    public required string TargetId { get; set; }
    public required string ChannelId { get; set; }
    public required OverwriteTargetType TargetType { get; set; }
    public Permission Allow { get; set; }
    public Permission Deny { get; set; }
}

public enum OverwriteTargetType
{
    Role = 0,
    Member = 1
}