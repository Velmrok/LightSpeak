using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationsService.src;

[Authorize]
public class AppHub : Hub<IAppHubClient>
{
}