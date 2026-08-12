using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common; 

public static class AuthConfig
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var authSettings = configuration.GetSection("AuthSettings").Get<AuthSettings>();
        ArgumentNullException.ThrowIfNull(authSettings, nameof(authSettings));
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authSettings.Authority;
            options.Audience = authSettings.Audience;
            options.RequireHttpsMetadata = false;
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var sub = context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub);

                    if (sub is null)
                    {
                        context.Fail("JWT does not contain required 'sub' claim.");
                    }
                    else if(string.IsNullOrWhiteSpace(sub.Value))
                    {
                        context.Fail("JWT 'sub' claim is empty.");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
public record AuthSettings
{
    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}

