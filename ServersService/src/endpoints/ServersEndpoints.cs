using System.IdentityModel.Tokens.Jwt;
using Common.Grpc;
using ServersService.src.dto;
using ServersService.src.services;
using Common.Mappers;


namespace ServersService.src.endpoints;
public static class ServersEndpoints
{
    public static IEndpointRouteBuilder MapServersEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", CreateServer).RequireAuthorization();

        return app;
    }
    // TODO : Max servers created by user based on role
    private static async Task<IResult> CreateServer(CreateServerRequest request, IServersApplicationService service, HttpContext ctx)
    {
        var userId = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value!;
        var result = await service.CreateServerAsync(request, userId, ctx.RequestAborted);
        if (result.IsError)
        {
            var (statusCode, body) = result.ToErrorResponse();
            return Results.Json(body, statusCode: statusCode);
        }
        var response = result.Value;
        return Results.Created($"{response.Id}", response);
    }
    
}