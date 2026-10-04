namespace LightSpeak.AppHost.src;

public static class NotificationsServiceConfiguration
{
    public static void ConfigureNotificationsService
    (this IResourceBuilder<ProjectResource> notificationsService, AppParameters p, AppSettings s,AppResources a)
    {
        notificationsService
            .WithReference(a.RabbitMQ)
            .WithEnvironment("AuthSettings__Authority", p.ClientAuthority)
            .WithEnvironment("AuthSettings__Audience", p.ClientAudience)
            .WaitFor(a.RabbitMQ);
    }
}