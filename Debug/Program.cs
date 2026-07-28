using Debug;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/token", (HttpRequest request) =>
{
    return Results.Ok(new TokenResponse(

        Jwt: request.Headers.Authorization.ToString().Replace("Bearer ", "")
    ));
});
app.UseHttpsRedirection();



app.Run();



