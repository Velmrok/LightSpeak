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
   
    private Permission CalculateOverWritedPermissions(Member? member, List<ChannelPermissionOverwrite>? overwrites)
    {
        var memberPerms = PermissionCalculator.CalculatePermissions(member);
        if (overwrites == null || overwrites.Count == 0) return memberPerms;

        var roleIds = member?.Roles.Select(r => r.Id).ToHashSet() ?? new HashSet<string>();
        Permission roleDeny = Permission.None, roleAllow = Permission.None;

        foreach (var overwrite in overwrites)
        {
            if (overwrite.TargetType == OverwriteTargetType.Role && roleIds.Contains(overwrite.TargetId)) 
            {
                roleDeny |= overwrite.Deny;
                roleAllow |= overwrite.Allow;
            }
        }
        memberPerms = (memberPerms & ~roleDeny) | roleAllow;

        var memberChannelOverwrite = overwrites.FirstOrDefault(o =>o.TargetType == OverwriteTargetType.Member && o.TargetId == member?.UserId);
        if (memberChannelOverwrite != null)
        {
            memberPerms = (memberPerms & ~memberChannelOverwrite.Deny) | memberChannelOverwrite.Allow;
        }
        return memberPerms;
    }
    public async Task<CallResult<List<string>>> FilterChannelMembersWithPermissionAsync(string channelId, Permission required, CancellationToken ct)
    {
        var channel = await db.Channels
            .AsNoTracking()
            .Include(c => c.PermissionOverwrites)
            .Include(c => c.Server)
            .FirstOrDefaultAsync(c => c.Id == channelId, ct);
        if (channel is null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Channel.NotFound", $"Channel with ID '{channelId}' not found.");
            return CallResult.Fail(section, error);
        }
        var channelOverwrites = channel.PermissionOverwrites;

        var members = await db.Members
            .AsNoTracking()
            .Include(m => m.Roles)
            .Where(m => m.ServerId == channel.ServerId)
            .ToListAsync(ct);

        var allowed = members
            .Where(m => (CalculateOverWritedPermissions(m, channelOverwrites) & required) == required)
            .Select(m => m.UserId)
            .ToList();

        return CallResult.Success(section, allowed);
    }
    public async Task<CallResult<List<string>>> FilterServerMembersWithPermissionAsync(string serverId, Permission required, CancellationToken ct)
    {
        var server = await db.Servers
            .AsNoTracking()
            .Include(s => s.Members)
                .ThenInclude(m => m.Roles)
            .FirstOrDefaultAsync(s => s.Id == serverId, ct);
        if (server is null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }

        var allowed = server.Members
            .Where(m => (PermissionCalculator.CalculatePermissions(m) & required) == required)
            .Select(m => m.UserId)
            .ToList();

        return CallResult.Success(section, allowed);
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
       
        
        var channelOverwrites = await db.Channels.Where(c => c.Id == channelId).Select(c => c.PermissionOverwrites).FirstOrDefaultAsync(ct);
        var memberPerms = CalculateOverWritedPermissions(member, channelOverwrites);

        return CallResult.Success(section, memberPerms);
    }

    public async Task<CallResult<Permission>> GetPermissionsByServerIdAsync(string userId, string serverId, CancellationToken ct)
    {
        var member = await db.Members.Include(m => m.Roles).FirstOrDefaultAsync(m => m.UserId == userId && m.ServerId == serverId, ct);
        if (member == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }
        return CallResult.Success(section, PermissionCalculator.CalculatePermissions(member));     
    }
}