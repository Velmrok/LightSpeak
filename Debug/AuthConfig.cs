using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Debug;

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

        });

        return services;
    }
}
public record AuthSettings
{
    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public bool RequireHttpsMetadata { get; init; } = true;
}
