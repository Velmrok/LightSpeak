namespace Common.Dto.Events;

public record UserLeftServerEvent(string UserId, string ServerId) : IEvent
{
    public static string RoutingKey => "user.left.server";
}