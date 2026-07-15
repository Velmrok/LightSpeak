using System.Net.Http.Headers;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.src.Extensions;

public static class YarpExtension
{
    public static void AddReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection("ReverseProxy"))
            .AddTransforms(builderContext =>
            {
                builderContext.AddRequestTransform(async transformContext =>
                {
                    var result = await transformContext.HttpContext.GetUserAccessTokenAsync();
                    if (result.Succeeded)
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.Token.AccessToken);
                    }
                });
            });

    }
}