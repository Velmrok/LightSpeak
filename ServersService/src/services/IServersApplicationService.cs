using Common.Dto;

using ServersService.src.dto;

namespace ServersService.src.services;

public interface IServersApplicationService
{
    Task<CallResult<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken);
    Task<CallResult<CreateChannelResponse>> CreateChannelAsync(string serverId, CreateChannelRequest request, string userId, CancellationToken cancellationToken);
    Task<CallResult<PostMessageResponse>> PostMessageAsync(string channelId, PostMessageRequest request, string userId, CancellationToken cancellationToken);
}