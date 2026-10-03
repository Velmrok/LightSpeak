using Common.Dto.Events;
using ServersService.src.services;

namespace ServersService.src;

public class UserDataChangeHandler
{
    private readonly IServersApplicationService _serversService;
    public UserDataChangeHandler(IServersApplicationService serversService)
    {
        _serversService = serversService;
    }

    public async Task Handle(UserDataChangedEvent evt)
    {
        await _serversService.UpdateUserDataAsync(evt.UserId, evt.Username, evt.AvatarUrl, CancellationToken.None);

    }
}