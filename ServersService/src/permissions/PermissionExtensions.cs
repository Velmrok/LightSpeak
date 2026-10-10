namespace ServersService.src.permissions;

public static class PermissionExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, Permission permission) where TBuilder : IEndpointConventionBuilder
        => builder.RequireAuthorization(policy => policy
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission)));
    public static Permission ToMask(this IEnumerable<Permission> perms) =>
        perms.Aggregate(Permission.None, (acc, p) => acc | p);
    public static List<Permission> ToList(this Permission perms)
    {
        var list = new List<Permission>();
        foreach (var perm in Enum.GetValues<Permission>())
        {
            var bits = (long)perm;
            if (bits != 0 && (bits & (bits - 1)) == 0 && perms.HasFlag(perm)) 
                list.Add(perm);
        }
        return list;
    }
}