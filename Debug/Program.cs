var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/token", (HttpRequest request) =>
{
    return Results.Ok(new
    {
        host = request.Host.ToString(),
        path = request.Path.ToString(),
        jwt = request.Headers.Authorization.ToString()
    });
});
app.UseHttpsRedirection();



app.Run();

