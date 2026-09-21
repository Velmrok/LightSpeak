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

        return app;
    }
    // TODO : Max servers created by user based on role
    private static async Task<IResult> CreateServer(CreateServerRequest request, IServersApplicationService service, HttpContext ctx,
     IResponseBuilderService responseBuilder)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.CreateServerAsync(request, userId, ctx.RequestAborted);
 
        var data = result.Data;
        return responseBuilder.BuildResponse(HttpStatusCode.Created, result,() => data!);

    }
    
}