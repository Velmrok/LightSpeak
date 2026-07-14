
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Gateway.src.Extensions;

public static class ServiceRegister
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddSingleton<ITicketStore, SessionStore>();
        return services;
    }
}