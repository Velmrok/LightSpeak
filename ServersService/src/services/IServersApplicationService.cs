using Common.Dto;

using ServersService.src.dto;

namespace ServersService.src.services;

public interface IServersApplicationService
{
    Task<CallResult<GetServerDetailsResponse>> GetServerDetailsAsync(string serverId, string userId, CancellationToken cancellationToken);
    Task<CallResult<GetChannelMessagesResponse>> GetChannelMessagesAsync(string channelId, int page, int limit, string userId, CancellationToken cancellationToken);
    Task<CallResult<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken);
    Task<CallResult<CreateChannelResponse>> CreateChannelAsync(string serverId, CreateChannelRequest request, string userId, CancellationToken cancellationToken);
    Task<CallResult<string>> GetOrCreateDmChannelAsync(string userId, string targetUserId, CancellationToken cancellationToken);

    Task<CallResult<PostMessageResponse>> PostMessageAsync(string channelId, PostMessageRequest request, string userId, CancellationToken cancellationToken);
    Task<CallResult> UpdateUserDataAsync(string userId, string username, string avatarUrl, CancellationToken cancellationToken);

    Task<CallResult<GetServersResponse>> GetServersAsync(string userId, CancellationToken cancellationToken);
}