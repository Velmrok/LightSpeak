using System.Net.Http.Headers;
using Duende.AccessTokenManagement.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.src.Extensions;

public static class YarpExtension
{
    public static IServiceCollection AddReverseProxyWithConfig(this IServiceCollection services, IConfiguration configuration)
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
                builderContext.AddResponseTransform(context =>
                {
                    var response = context.ProxyResponse;
                    

                    if (response?.Headers.Location is not { } location)
                        return default;

                    if (location.IsAbsoluteUri)
                        return default;

                    var path = builderContext.Route.Match.Path;
                    var prefix = path?.Split("/{**", StringSplitOptions.None)[0] ?? "";
                
                    var rewritten = prefix + "/" + location.OriginalString;
                    context.HttpContext.Response.Headers.Location = rewritten;

                    return default;
                });
            })
            .AddServiceDiscoveryDestinationResolver();
        return services;
    }
}