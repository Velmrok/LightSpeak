using System.IdentityModel.Tokens.Jwt;
using ServersService.src.dto;
using ServersService.src.services;
using Common.Services;
using System.Net;


namespace ServersService.src.endpoints;
public static class ServersEndpoints
{
    public static IEndpointRouteBuilder MapServersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateServer).RequireAuthorization();
        app.MapPost("/{serverId}/channels", CreateChannel).RequireAuthorization();

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
    
}