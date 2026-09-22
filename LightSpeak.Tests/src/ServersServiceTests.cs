using System.Net.Http.Json;
using Aspire.Hosting;
using Common.Dto;
using LightSpeak.AppHost.src.Constants;
using ServersService.src;
using ServersService.src.dto;
using Microsoft.EntityFrameworkCore;

namespace LightSpeak.Tests.src;
[Collection("Aspire")]
public class ServersServiceTest : TestBase
{
     public ServersServiceTest(AppFixture fixture) : base(fixture)
    {
       
    }
    private async Task<AppDbContext> CreateDbContext(CancellationToken ct)
    {
        return await Fixture.CreateDbContextAsync<AppDbContext>(ResourcesNames.ServersDatabase, ct);
    }
    [Fact]
    public async Task CreatesServer_Correctly_OnCreateServerRequestWhileBeingLoggedIn()
    {
        var ct = CancellationToken.None;

        var client = Fixture.CreateGatewayClient();

        await _authClient.LoginAsync(client, DefaultTimeout, ct);

        var request = new CreateServerRequest
        (
            Name: "Test Server"
        );

        var json = await client.PostAsJsonAsync("/servers", request, ct);
        Assert.Equal(HttpStatusCode.Created, json.StatusCode);
        
        var response = await ReadFromJson<CreateServerResponse>(json, ct);
        Assert.NotNull(response.Data);
        Assert.Null(response.Errors);
        
        var data = response.Data;
        Assert.Equal(request.Name, data.Name);
      
        var dbContext = await CreateDbContext(ct);

        var serverInDb = await dbContext.Servers.FirstOrDefaultAsync(s => s.Id == data.Id, ct);
        Assert.NotNull(serverInDb);
        Assert.Equal(request.Name, serverInDb.Name);


    }
}
   