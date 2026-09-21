using Common.Errors;
using Common.Grpc;
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
        return new AppError(statusCode.ToDomainErrorCode(), code, details);
    }
    public static RpcException ToRpcException(this AppError error)
    {
        var trailers = new Metadata
        {
            { AppError.ErrorTitleKey, error.Code }
        };
        return new RpcException(new Status(error.StatusCode.ToGrpc(), error.Details ?? string.Empty), trailers);
    }
}