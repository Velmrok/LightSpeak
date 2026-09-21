using Common.Errors;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Common.Mappers;
public static class DomainErrorCodeMapper
{
    public static int ToHttp(this DomainErrorCode kind) => kind switch
    {
        DomainErrorCode.Validation      => StatusCodes.Status400BadRequest,
        DomainErrorCode.BusinessRule    => StatusCodes.Status422UnprocessableEntity,
        DomainErrorCode.Unauthenticated => StatusCodes.Status401Unauthorized,
        DomainErrorCode.Forbidden       => StatusCodes.Status403Forbidden,
        DomainErrorCode.NotFound        => StatusCodes.Status404NotFound,
        DomainErrorCode.Conflict        => StatusCodes.Status409Conflict,
        DomainErrorCode.RateLimited     => StatusCodes.Status429TooManyRequests,
        DomainErrorCode.Cancelled       => StatusCodes.Status499ClientClosedRequest,
        DomainErrorCode.NotImplemented  => StatusCodes.Status501NotImplemented,
        DomainErrorCode.Unavailable     => StatusCodes.Status503ServiceUnavailable,
        DomainErrorCode.Timeout         => StatusCodes.Status504GatewayTimeout,
        _                         => StatusCodes.Status500InternalServerError
    };
    public static StatusCode ToGrpc(this DomainErrorCode kind) => kind switch
    {
        DomainErrorCode.Validation => StatusCode.InvalidArgument,
        DomainErrorCode.BusinessRule => StatusCode.FailedPrecondition,
        DomainErrorCode.Unauthenticated => StatusCode.Unauthenticated,
        DomainErrorCode.Forbidden => StatusCode.PermissionDenied,
        DomainErrorCode.NotFound => StatusCode.NotFound,
        DomainErrorCode.Conflict => StatusCode.AlreadyExists,
        DomainErrorCode.RateLimited => StatusCode.ResourceExhausted,
        DomainErrorCode.Cancelled => StatusCode.Cancelled,
        DomainErrorCode.NotImplemented => StatusCode.Unimplemented,
        DomainErrorCode.Unavailable => StatusCode.Unavailable,
        DomainErrorCode.Timeout => StatusCode.DeadlineExceeded,
        _ => StatusCode.Internal
    };
     public static DomainErrorCode ToDomainErrorCode(this StatusCode code) => code switch
    {
        StatusCode.InvalidArgument   => DomainErrorCode.Validation,
        StatusCode.OutOfRange        => DomainErrorCode.Validation,
        StatusCode.FailedPrecondition => DomainErrorCode.BusinessRule,
        StatusCode.Unauthenticated   => DomainErrorCode.Unauthenticated,
        StatusCode.PermissionDenied  => DomainErrorCode.Forbidden,
        StatusCode.NotFound          => DomainErrorCode.NotFound,
        StatusCode.AlreadyExists     => DomainErrorCode.Conflict,
        StatusCode.Aborted           => DomainErrorCode.Conflict,
        StatusCode.ResourceExhausted => DomainErrorCode.RateLimited,
        StatusCode.Cancelled         => DomainErrorCode.Cancelled,
        StatusCode.Unimplemented     => DomainErrorCode.NotImplemented,
        StatusCode.Unavailable       => DomainErrorCode.Unavailable,
        StatusCode.DeadlineExceeded  => DomainErrorCode.Timeout,
        _                            => DomainErrorCode.Internal 
    };
}