using Common.Constants;
using Common.Dto;
using Common.Errors;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using ServersService.src.permissions;

namespace ServersService.src;

public class AuthResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Succeeded)
        {
            await _default.HandleAsync(next, context, policy, authorizeResult);
            return;
        }
        CallResult callResult;
        if (context.Items.TryGetValue(PermissionHandler.ErrorKey, out var o) && o is CallResult result)
        {
            callResult = result;
        }
        else if (authorizeResult.Forbidden && context.User.Identity?.IsAuthenticated == true)
        {
            var error = new AppError(DomainErrorCode.Forbidden, "Forbidden", "You do not have permission to access this resource.");
            callResult = CallResult.Fail(ResourcesSectionNames.Servers, error);
        }
        else
        {
            var error = new AppError(DomainErrorCode.Unauthenticated, "Unauthenticated", "This resource requires authentication. Please log in and try again.");
            callResult = CallResult.Fail(ResourcesSectionNames.Servers, error);
        }
        var builder = context.RequestServices.GetRequiredService<IResponseBuilderService>();
        var response = builder.BuildError(callResult);
        await response.ExecuteAsync(context);

    
    }
}
