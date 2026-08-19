using Duende.AccessTokenManagement.OpenIdConnect;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Common;

public static class GrpcClientExtensions
{
    public static IHttpClientBuilder ConfigureGrpcCredentials(this IHttpClientBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder
        .AddCallCredentials(async (context, metadata, serviceProvider) =>
            {
                var accessor =
                    serviceProvider.GetRequiredService<IHttpContextAccessor>();

                var httpContext = accessor.HttpContext;

                if (httpContext is null)
                    return;

                var token = httpContext.Request.Headers.Authorization.ToString();

                metadata.Add("Authorization",token);
            })
        .ConfigureChannel(options =>
            {
                options.UnsafeUseInsecureChannelCallCredentials = true;
            });
        return builder;
    }
}