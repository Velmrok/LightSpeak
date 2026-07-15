using System.Net.Http.Headers;
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
                    string? token = await transformContext.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
                    if (token is not null)
                    {
                        transformContext.ProxyRequest.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", token);
                    }
                });
            });
         
    }
}