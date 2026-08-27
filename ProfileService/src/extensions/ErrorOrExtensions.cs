using Common.Grpc;
using ErrorOr;
using Grpc.Core;

namespace ProfileService.src.extensions;

public static class ErrorOrExtensions
{
    public static RpcException ToRpcException(this Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCode.InvalidArgument,
            ErrorType.NotFound => StatusCode.NotFound,
            ErrorType.Conflict => StatusCode.AlreadyExists,
            ErrorType.Unauthorized => StatusCode.Unauthenticated,
            ErrorType.Forbidden => StatusCode.PermissionDenied,
            _ => StatusCode.Internal
        };

        var appError = new AppError(statusCode, error.Code, error.Description);
        return appError.ToRpcException();
    }
}