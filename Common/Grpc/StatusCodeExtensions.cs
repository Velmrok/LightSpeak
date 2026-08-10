using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Grpc;

public static class StatusCodeExtensions
{
    public static IResult ToRestResponse(this StatusCode statusCode, object? data = null)
    {
        return Results.Json(
            data,
            statusCode: (int)statusCode.ToHttpStatusCode());
    }
    // Placeholder error mapping!!!!!
    public static HttpStatusCode ToHttpStatusCode(this StatusCode statusCode)
    {
        return statusCode switch
        {
            StatusCode.OK => HttpStatusCode.OK,
            StatusCode.Cancelled => HttpStatusCode.BadRequest,
            StatusCode.Unknown => HttpStatusCode.InternalServerError,
            StatusCode.InvalidArgument => HttpStatusCode.BadRequest,
            StatusCode.DeadlineExceeded => HttpStatusCode.RequestTimeout,
            StatusCode.NotFound => HttpStatusCode.NotFound,
            StatusCode.AlreadyExists => HttpStatusCode.Conflict,
            StatusCode.PermissionDenied => HttpStatusCode.Forbidden,
            StatusCode.ResourceExhausted => HttpStatusCode.TooManyRequests,
            StatusCode.FailedPrecondition => HttpStatusCode.BadRequest,
            StatusCode.Aborted => HttpStatusCode.Conflict,
            StatusCode.OutOfRange => HttpStatusCode.BadRequest,
            StatusCode.Unimplemented => HttpStatusCode.NotImplemented,
            StatusCode.Internal => HttpStatusCode.InternalServerError,
            StatusCode.Unavailable => HttpStatusCode.ServiceUnavailable,
            StatusCode.DataLoss => HttpStatusCode.InternalServerError,
            StatusCode.Unauthenticated => HttpStatusCode.Unauthorized,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode), $"Unexpected status code: {statusCode}")
        };
    }
  
}