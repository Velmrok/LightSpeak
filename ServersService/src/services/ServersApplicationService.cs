
using Common.Clients;
using Microsoft.EntityFrameworkCore;
using ServersService.src.dto;
using ServersService.src.models;
using Common.Mappers;
using Common.Dto;
using Common.Constants;
using Common.Errors;
using Common.Services;
using Common.Dto.Events;

namespace ServersService.src.services;

public class ServersApplicationService(AppDbContext db, IProfileClient profileClient, IEventPublisher eventPublisher) : IServersApplicationService
{
    private const string section = ResourcesSectionNames.Servers;

    public async Task<CallResult<CreateChannelResponse>> CreateChannelAsync(string serverId, CreateChannelRequest request, string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var error = new AppError(DomainErrorCode.Validation,"InvalidChannelName", "Channel name cannot be empty or whitespace.");
            return new (section, null, error);
        }
        
        var server = await db.Servers.Include(s => s.Members).FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken);
        
        // Even if the server exists, return 404 for unauthorized users to avoid revealing the existence of the server.
        if (server == null || !server.Members.Any(m => m.UserId == userId))
        {
            var error = new AppError(DomainErrorCode.NotFound, "ServerNotFound", $"Server with ID '{serverId}' not found.");
            return new (section, null, error);
        }

        var channel = new Channel
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            ServerId = server.Id
        };

        db.Channels.Add(channel);
        await db.SaveChangesAsync(cancellationToken);

        var response = new CreateChannelResponse
        (
            Name : channel.Name,
            ChannelId : channel.Id
        );
        return new(ResourcesSectionNames.Servers, response, null);
    }

    public async Task<CallResult<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            var error = new AppError(DomainErrorCode.Validation,"InvalidServerName", "Server name cannot be empty or whitespace.");
            return new (section, null, error);
        }
        var user = await GetUserSnapshot(userId, cancellationToken);
        if (!user.IsSuccess())
        {
            return user.Fail<CreateServerResponse>();
        }
        
        var server = new Server
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
        };
        var newMember = new Member
        {
            UserId = userId,
            ServerId = server.Id,
            Server = server
        };
        server.Members.Add(newMember);

        db.Servers.Add(server);
        await db.SaveChangesAsync(cancellationToken);
        var response = new CreateServerResponse
        (
            Name : server.Name,
            ServerId : server.Id
        );
        return new(ResourcesSectionNames.Servers, response, null);
    }
    private async Task<CallResult<UserSnapshot>> GetUserSnapshot(string userId, CancellationToken cancellationToken)
    {
        var user = await db.UserSnapshots.FirstOrDefaultAsync(m => m.Id == userId, cancellationToken);
        if (user == null)
        {
            var callResult = await profileClient.GetUserSnapshotAsync(userId, cancellationToken);
            if (!callResult.IsSuccess())
            {
                return callResult.Fail<UserSnapshot>();
            }
            var userSnapshot = callResult.Data;
            ArgumentNullException.ThrowIfNull(userSnapshot, nameof(userSnapshot));

            var newUser = new UserSnapshot
            {
                Id = userSnapshot.UserId,
                Name = userSnapshot.Username,
                AvatarUrl = userSnapshot.AvatarUrl
            };
           
            db.UserSnapshots.Add(newUser);
        }
        return new(ResourcesSectionNames.Servers, user, null);
    }

    public async Task<CallResult<PostMessageResponse>> PostMessageAsync(string channelId, PostMessageRequest request, string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            var error = new AppError(DomainErrorCode.Validation,"InvalidMessageContent", "Message content cannot be empty or whitespace.");
            return new (section, null, error);
        }

        var channel = await db.Channels.Include(c => c.Server).ThenInclude(s => s.Members).FirstOrDefaultAsync(c => c.Id == channelId, cancellationToken);
        
        // Even if the channel exists, return 404 for unauthorized users to avoid revealing the existence of the channel.
        if (channel == null || !channel.Server.Members.Any(m => m.UserId == userId))
        {
            var error = new AppError(DomainErrorCode.NotFound, "ChannelNotFound", $"Channel with ID '{channelId}' not found.");
            return new (section, null, error);
        }

        var message = new ChatMessage
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = userId,
            Content = request.Content,
            ChannelId = channel.Id
        };

        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        var response = new PostMessageResponse
        (
            Content: message.Content,
            MessageId: message.Id
        );
        var senderSnapshotResult = await GetUserSnapshot(userId, cancellationToken);
        if (!senderSnapshotResult.IsSuccess()) return senderSnapshotResult.Fail<PostMessageResponse>();

        if (senderSnapshotResult.Data == null)
             throw new InvalidOperationException("Sender snapshot data cannot be null after successful CallResult.");

        var senderSnapshot = new SenderSnapshot(senderSnapshotResult.Data.Id, senderSnapshotResult.Data.Name, senderSnapshotResult.Data.AvatarUrl);
        await eventPublisher.PublishEventAsync(new MessageCreatedEvent(message.Id, message.ChannelId, senderSnapshot, message.Content, message.Timestamp), cancellationToken);
        return new(section, response, null); 
    }
}