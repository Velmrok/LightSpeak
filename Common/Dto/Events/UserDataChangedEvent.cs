namespace Common.Dto.Events;

public record UserDataChangedEvent(string UserId, string Username, string AvatarUrl) : IEvent
{
    public static string RoutingKey => "user.data.changed";
}