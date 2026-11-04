public class UserServerPosition
{
    public required string UserId { get; set; }
    public required string ServerId { get; set; }
    public required int Position { get; set; }

    public Profile Profile { get; set; } = null!;
}