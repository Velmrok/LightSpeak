namespace ServersService.src.permissions;

public static class PermissionExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, Permission permission) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(policy => policy
            .RequireAuthenticatedUser() 
            .AddRequirements(new PermissionRequirement(permission)));
}