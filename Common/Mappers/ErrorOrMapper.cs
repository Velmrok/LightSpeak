using Common.Dto;
using Common.Grpc;
using ErrorOr;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Common.Mappers;

// TODO : Create proper mapping
public static class ErrorOrMapper
{
    public static int ToHttpStatusCode(this ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Failure => StatusCodes.Status500InternalServerError,
        ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status500InternalServerError
    };
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
    public static (int StatusCode, ErrorResponse Body) ToErrorResponse<T>(this ErrorOr<T> result)
    {
        var errors = result.Errors;
        var e = errors[0];
        return (ToHttpStatusCode(e.Type), new ErrorResponse(e.Code, e.Description));   
    }

}