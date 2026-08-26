using Microsoft.Extensions.Configuration;

namespace LightSpeak.AppHost.src;

public static class ConfigurationExtensions
{
    public static AppParameters AddApplicationParameters(this IDistributedApplicationBuilder builder)
    {
        return new()
        {
            KcAdminUser = builder.AddParameter("kc-admin-user"),
            KcAdminPassword = builder.AddParameter("kc-admin-password"/*, secret: true*/),
            KcGatewaySecret = builder.AddParameter("kc-gateway-secret"/*, secret: true*/),
            KcAdminSecret = builder.AddParameter("kc-admin-client-secret"/*, secret: true*/),
            AppBaseUrl = builder.AddParameter("app-base-url"),
            ClientAudience = builder.AddParameter("client-audience"),
            RabbitUser = builder.AddParameter("rabbit-user"),
            RabbitPassword = builder.AddParameter("rabbit-password"/*, secret: true*/),
            PostgresUser = builder.AddParameter("postgres-user"),
            PostgresPassword = builder.AddParameter("postgres-password"/*, secret: true*/)

        };
    }
    public static AppSettings AddApplicationSettings(this IDistributedApplicationBuilder builder)
    {
        return new()
        {
            IsTesting = builder.Configuration.GetValue<bool>("IsTesting"),
            IsDev = builder.Configuration.GetValue<bool>("IsDev")
        };
    }
}