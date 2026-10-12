using System.IdentityModel.Tokens.Jwt;
using ServersService.src.dto;
using ServersService.src.services;
using Common.Services;
using System.Net;
using Common.Dto;
using ServersService.src.permissions;


namespace ServersService.src.endpoints;
public static class ServersEndpoints
{
    public static IEndpointRouteBuilder MapServersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateServer).RequireAuthorization();
        app.MapPost("/{serverId}/channels", CreateChannel).RequireAuthorization();
        app.MapPatch("/{serverId}/roles", PatchRole).RequirePermission(Permission.ManageRoles);
        app.MapPost("/{serverId}/roles", CreateRole).RequirePermission(Permission.ManageRoles);
        app.MapGet("/{serverId}/roles", GetRoles).RequireAuthorization();
        app.MapDelete("/{serverId}/roles/{roleId}", DeleteRole).RequirePermission(Permission.ManageRoles);

        app.MapPost("/channels/{channelId}/messages", PostMessage).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> CreateServer(CreateServerRequest request, IServersApplicationService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.CreateServerAsync(request, userId, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.Created, result,() => data!);

    }

    private static async Task<IResult> CreateChannel(string serverId, CreateChannelRequest request, IServersApplicationService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.CreateChannelAsync(serverId, request, userId, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.Created, result,() => data!);

    }
    
    private static async Task<IResult> PostMessage(string channelId, PostMessageRequest request, IServersApplicationService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.PostMessageAsync(channelId, request, userId, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.Created, result,() => data!);

    }
    private static async Task<IResult> PatchRole(string serverId, PatchRoleRequest request, IRoleService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.PatchRoleAsync(serverId,userId, request, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.OK, result,() => data!);

    }
    private static async Task<IResult> GetRoles(string serverId,  IRoleService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.GetRolesAsync(serverId,userId,  ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.OK, result,() => data!);


    }
    private static async Task<IResult> CreateRole(string serverId, CreateRoleRequest request, IRoleService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.CreateRoleAsync(serverId,userId, request, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.Created, result,() => data!);

    }
    private static async Task<IResult> DeleteRole(string serverId, string roleId, IRoleService service,
     HttpContext ctx,IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.DeleteRoleAsync(serverId,userId, roleId, ctx.RequestAborted);

        return responseBuilder.BuildResponse(result);  

    }
    
    
    
}