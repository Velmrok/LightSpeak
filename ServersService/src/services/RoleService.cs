using Common.Constants;
using Common.Dto;
using Common.Errors;
using Microsoft.EntityFrameworkCore;
using ServersService.src.dto;
using ServersService.src.permissions;

namespace ServersService.src.services;

public class RoleService(AppDbContext db) : IRoleService
{
    private const string section = ResourcesSectionNames.Servers;
    public async Task<CallResult<GetRolesResponse>> GetRolesAsync(string serverId, string userId, CancellationToken ct)
    {
        var member = await db.Members.FirstOrDefaultAsync(u => u.UserId == userId && u.ServerId == serverId, cancellationToken: ct);
        var server = await db.Servers.FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken: ct);
        if (member == null || server == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }
        var roles = await db.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct) ?? [];
        var response = new GetRolesResponse([.. roles.Select(r => new RoleDto(r.Id, r.Name, r.Permissions.ToList()))]);
        return CallResult.Success(section, response);
    }

    public async Task<CallResult<PatchRoleResponse>> PatchRoleAsync(string serverId, string userId, PatchRoleRequest request, CancellationToken ct)
    {
        var member = await db.Members.Include(m => m.Roles).FirstOrDefaultAsync(u => u.UserId == userId && u.ServerId == serverId, cancellationToken: ct);
        var server = await db.Servers.FirstOrDefaultAsync(s => s.Id == serverId, cancellationToken: ct);
        if (member == null || server == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }
        var roleToEdit = await db.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId && r.ServerId == serverId, cancellationToken: ct);
        if (roleToEdit == null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Role.NotFound", $"Role with ID '{request.RoleId}' not found.");
            return CallResult.Fail(section, error);
        }
        var memberRole = member.Roles.GetMostPowerfulRole();
        if (memberRole == null || memberRole.Priority >= roleToEdit.Priority)
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Role.Edit.Unauthorized", $"User with ID '{userId}' does not have permission to edit this role.");
            return CallResult.Fail(section, error);
        }
        if(roleToEdit.Name == "Everyone" && request.NewName != null)
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Role.Edit.Unauthorized", $"Name of the role 'Everyone' cannot be changed.");
            return CallResult.Fail(section, error);
        }
        roleToEdit.Name = request.NewName ?? roleToEdit.Name;

        var add = request.AddedPermissions?.ToMask() ?? Permission.None;
        var remove = request.RemovedPermissions?.ToMask() ?? Permission.None;
        roleToEdit.Permissions |= add;
        roleToEdit.Permissions &= ~remove;

        await db.SaveChangesAsync(ct);
        var response = new PatchRoleResponse(roleToEdit.Id, roleToEdit.Name, roleToEdit.Permissions.ToList());
        return CallResult.Success(section, response);
    }

    
}