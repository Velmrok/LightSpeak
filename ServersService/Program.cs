using Common;
using Microsoft.EntityFrameworkCore;
using ServersService.src;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("servers-database")));
    
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();




app.MapHealthChecks("/health");



app.Run();

