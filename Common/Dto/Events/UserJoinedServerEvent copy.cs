namespace Common.Dto.Events;

public record UserJoinedServerEvent(string UserId, string ServerId) : IEvent
{
    public static string RoutingKey => "user.joined.server";
}