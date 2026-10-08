using Common.Constants;
using Common.Dto;
using Common.Errors;
using Microsoft.EntityFrameworkCore;
using ServersService.src.models;
using ServersService.src.permissions;

namespace ServersService.src.services;

public class PermissionService(AppDbContext db) : IPermissionService
{
    private const string section = ResourcesSectionNames.Servers;
    private Permission CalculatePermissions(Member? member)
    {
        Permission perms = Permission.None;
        if (member != null)
        {
            foreach (var role in member.Roles)
            {
                perms |= role.Permissions;
            }
        }
        return perms;
    }
    public async Task<CallResult<Permission>> GetPermissionsByChannelIdAsync(string userId, string channelId, CancellationToken ct)
    {
        var serverId = await db.Channels.Where(c => c.Id == channelId).Select(c => c.ServerId).FirstOrDefaultAsync(ct);
        var member = await db.Members.Include(m => m.Roles).FirstOrDefaultAsync(m => m.UserId == userId && m.ServerId == serverId, ct);
        if(serverId == null || member == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Channel.NotFound", $"Channel with ID '{channelId}' not found.");
            return CallResult.Fail(section, error);
        }
       
        var memberPerms = CalculatePermissions(member);
        var channelOverwrites = await db.Channels.Where(c => c.Id == channelId).Select(c => c.PermissionOverwrites).FirstOrDefaultAsync(ct);
        if (channelOverwrites == null || channelOverwrites.Count == 0) return CallResult.Success(section, memberPerms);

        var roleIds = member.Roles.Select(r => r.Id).ToHashSet();
        Permission roleDeny = Permission.None, roleAllow = Permission.None;

        foreach (var overwrite in channelOverwrites)
        {
            if (overwrite.TargetType == OverwriteTargetType.Role && roleIds.Contains(overwrite.TargetId)) 
            {
                roleDeny |= overwrite.Deny;
                roleAllow |= overwrite.Allow;
            }
        }
        memberPerms = (memberPerms & ~roleDeny) | roleAllow;

        var memberChannelOverwrite = channelOverwrites.FirstOrDefault(o =>o.TargetType == OverwriteTargetType.Member && o.TargetId == member.UserId);
        if (memberChannelOverwrite != null)
        {
            memberPerms = (memberPerms & ~memberChannelOverwrite.Deny) | memberChannelOverwrite.Allow;
        }
        return CallResult.Success(section, memberPerms);
    }

    public async Task<CallResult<Permission>> GetPermissionsByServerIdAsync(string userId, string serverId, CancellationToken ct)
    {
        var member = await db.Members.FirstOrDefaultAsync(m => m.UserId == userId && m.ServerId == serverId, ct);
        if (member == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }
        return CallResult.Success(section, CalculatePermissions(member));     
    }
}