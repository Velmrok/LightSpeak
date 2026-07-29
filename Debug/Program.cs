using Debug;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuth(builder.Configuration);

var app = builder.Build();

app.MapGet("/token", (HttpRequest request) =>
{
    return Results.Ok(new TokenResponse(

        Jwt: request.Headers.Authorization.ToString().Replace("Bearer ", "")
    ));
}).RequireAuthorization();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();



