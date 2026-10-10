using Common.Constants;
using Common.Dto;
using Common.Errors;
using Microsoft.EntityFrameworkCore;
using ServersService.src.dto;
using ServersService.src.models;
using ServersService.src.permissions;

namespace ServersService.src.services;

public class RoleService(AppDbContext db) : IRoleService
{
    private const string section = ResourcesSectionNames.Servers;
    private async Task<CallResult<(Server Server, Member Member)>> GetServerAndMemberAsync(string serverId, string userId, CancellationToken ct)
    {
        var member = await db.Members
            .Include(m => m.Server)
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ServerId == serverId, ct);

        if (member is null || member.Server is null)
        {
            var error = new AppError(DomainErrorCode.NotFound, "Server.NotFound", $"Server with ID '{serverId}' not found.");
            return CallResult.Fail(section, error);
        }
           

        return CallResult.Success(section, (member.Server, member));
    }

    public async Task<CallResult<CreateRoleResponse>> CreateRoleAsync(string serverId,string userId, CreateRoleRequest request, CancellationToken ct)
    {
       var serverAndMemberResult = await GetServerAndMemberAsync(serverId, userId, ct);
        if (!serverAndMemberResult.IsSuccess) return serverAndMemberResult.AsFailure();
        var (server, member) = serverAndMemberResult.Data;

        var perms = PermissionCalculator.CalculatePermissions(member);
        if (!perms.HasFlag(Permission.ManageRoles))
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Role.Create.Forbidden", $"User with ID '{userId}' does not have permission to create roles.");
            return CallResult.Fail(section, error);
        }
        var memberRole = member.Roles.GetMostPowerfulRole();
        if (memberRole == null || memberRole.Priority >= request.Priority)
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Role.Create.Forbidden", $"User with ID '{userId}' does not have permission to create a role with priority '{request.Priority}'.");
            return CallResult.Fail(section, error);
        }
        var newRolePerms = request.Permissions.ToMask();
        if ((newRolePerms & ~perms) != Permission.None) 
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Role.Create.Forbidden", $"User with ID '{userId}' does not have permission to assign the requested permissions.");
            return CallResult.Fail(section, error);
        }
        var serverRoles = await db.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct);
        if(serverRoles.Any(r => r.Name == request.Name))
        {
            var error = new AppError(DomainErrorCode.Conflict, "Role.Create.DuplicateName", $"A role with the name '{request.Name}' already exists in the server.");
            return CallResult.Fail(section, error);
        }
        if(serverRoles.Any(r => r.Priority == request.Priority))
        {
            serverRoles.ForEach(r => {
                if(r.Priority >= request.Priority)
                {
                    r.Priority++;
                }
            });
        }
        var newRole = new Role
        {
            Name = request.Name,
            Priority = request.Priority,
            Permissions = newRolePerms,
            ServerId = serverId
        };
        db.Roles.Add(newRole);
        await db.SaveChangesAsync(ct);
        var response = new CreateRoleResponse(newRole.Id, newRole.Name, newRole.Priority, newRole.Permissions.ToList());
        return CallResult.Success(section, response);
    }

    public async Task<CallResult<GetRolesResponse>> GetRolesAsync(string serverId, string userId, CancellationToken ct)
    {
        var serverAndMemberResult = await GetServerAndMemberAsync(serverId, userId, ct);
        if (!serverAndMemberResult.IsSuccess) return  serverAndMemberResult.AsFailure();

        var roles = await db.Roles.Where(r => r.ServerId == serverId).ToListAsync(ct) ?? [];
        var response = new GetRolesResponse([.. roles.Select(r => new RoleDto(r.Id, r.Name, r.Permissions.ToList()))]);
        return CallResult.Success(section, response);
    }

    public async Task<CallResult<PatchRoleResponse>> PatchRoleAsync(string serverId, string userId, PatchRoleRequest request, CancellationToken ct)
    {
        var serverAndMemberResult = await GetServerAndMemberAsync(serverId, userId, ct);
        if (!serverAndMemberResult.IsSuccess) return serverAndMemberResult.AsFailure();
        var (server, member) = serverAndMemberResult.Data;

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