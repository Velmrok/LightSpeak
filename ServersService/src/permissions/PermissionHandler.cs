using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Common.Dto;
using Microsoft.AspNetCore.Authorization;
using ServersService.src.services;

namespace ServersService.src.permissions;

public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissions;
    public const string ErrorKey = "permission-error";

    public PermissionHandler(IPermissionService permissions) => _permissions = permissions;
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.Resource is not HttpContext http) return;

        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (userId is null) return;

        var route = http.Request.RouteValues;

        CallResult<Permission> result;
        if (route.TryGetValue("channelId", out var ch) && ch is string channelId)
            result = await _permissions.GetPermissionsByChannelIdAsync(userId, channelId, http.RequestAborted);
        else if (route.TryGetValue("serverId", out var sv) && sv is string serverId)
            result = await _permissions.GetPermissionsByServerIdAsync(userId, serverId, http.RequestAborted);
        else return;

        if (!result.IsSuccess)
        {
            http.Items[ErrorKey] = result;
            context.Fail();
            return;
        }

        if (result.Data.HasFlag(requirement.Permission))
            context.Succeed(requirement);
        return;
    }
}