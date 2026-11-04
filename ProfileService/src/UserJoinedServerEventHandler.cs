using Common.Dto;
using Common.Dto.Events;
using MassTransit;
using ProfileService.src.database;
using ProfileService.src.services;

namespace ProfileService.src;

public class UserJoinedServerEventHandler
{
    private readonly IServersProfileService _serversProfileService;
    public UserJoinedServerEventHandler(IServersProfileService serversProfileService)
    {
        _serversProfileService = serversProfileService;
    }
   
    public async Task Handle(UserJoinedServerEvent evt)
    {
        var result = await _serversProfileService.AddNewUserServerPositionAsync(evt.UserId, evt.ServerId, CancellationToken.None);
       

        if (!result.IsSuccess)
            return; // LOGGING
        

        
    }
}