using Common.Grpc;
using ErrorOr;
using Grpc.Core;

namespace Common.Mappers;

// TODO : Create proper mapping
public static class AppErrorMapper
{
    public static AppError ToAppError(this RpcException ex)
    {
        
        var statusCode = ex.StatusCode;

        var code = ex.Trailers.GetValue(AppError.ErrorTitleKey)
         ?? ex.StatusCode switch
         {
             StatusCode.DeadlineExceeded => "service-timeout",
             StatusCode.Unavailable => "service-unavailable",
             StatusCode.Unauthenticated => "unauthenticated",
             StatusCode.PermissionDenied => "permission-denied",
             StatusCode.ResourceExhausted => "rate-limited",
             StatusCode.Cancelled => "request-cancelled",
             _ => "unexpected"
         };

        var details = string.IsNullOrWhiteSpace(ex.Status.Detail)
        ? $"gRPC call failed with {ex.StatusCode}"
        : ex.Status.Detail;
        return new AppError(statusCode, code, details);
    }
    public static RpcException ToRpcException(this AppError error)
    {
        var trailers = new Metadata
        {
            { AppError.ErrorTitleKey, error.Code }
        };
        return new RpcException(new Status(error.StatusCode, error.Details ?? string.Empty), trailers);
    }
    public static Error ToErrorOr(this AppError error)
    {
        return error.StatusCode switch
        {
            StatusCode.Unauthenticated => Error.Unauthorized(error.Code, error.Details),
            StatusCode.PermissionDenied => Error.Forbidden(error.Code, error.Details),
            StatusCode.InvalidArgument => Error.Validation(error.Code, error.Details),
            StatusCode.ResourceExhausted => Error.Custom(429, error.Code, error.Details),
            StatusCode.Unknown => Error.Failure(error.Code, error.Details),
            _ => Error.Unexpected(error.Code, error.Details)
        };
    }
}