
using Common.Clients;
using Microsoft.EntityFrameworkCore;
using ServersService.src.dto;
using ServersService.src.models;
using Common.Mappers;
using Common.Dto;
using Common.Constants;

namespace ServersService.src.services;

public class ServersApplicationService(AppDbContext db, IProfileClient profileClient) : IServersApplicationService
{

    public async Task<CallResult<CreateServerResponse>> CreateServerAsync(CreateServerRequest request,string userId, CancellationToken cancellationToken)
    {
        
        var user = await db.UserSnapshots.FirstOrDefaultAsync(m => m.Id == userId, cancellationToken);
        if (user == null)
        {
            var callResult = await profileClient.GetUserSnapshotAsync(userId, cancellationToken);
            if (!callResult.IsSuccess())
            {
                return callResult.Fail<CreateServerResponse>();
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
            Id : server.Id
        );
        return new(ResourcesSectionNames.Servers, response, null);
    }
}