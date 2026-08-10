using Grpc.Core;

namespace Common.Grpc;
public class AppError{
    public StatusCode StatusCode { get; init; }
    public string Code { get; init; }
    public string Details { get; init; }
    public const string ErrorTitleKey = "error-code";
    public AppError(StatusCode statusCode, string code, string details)
    {
        StatusCode = statusCode;
        Code = code;
        Details = details;
    }
    public AppError(RpcException ex)
    {
        StatusCode = ex.StatusCode;
        
        Code = ex.Trailers.GetValue(ErrorTitleKey)
        ?? ex.StatusCode switch
        {
            StatusCode.Unauthenticated => "unauthenticated",
            StatusCode.PermissionDenied => "permission-denied",
            _ => "unexpected"
        };

        Details = ex.Status.Detail;
    }
    public RpcException ToRpcException()
    {
        var trailers = new Metadata
        {
            { ErrorTitleKey, Code }
        };
        return new RpcException(new Status(StatusCode, Details ?? string.Empty), trailers);
    }
}