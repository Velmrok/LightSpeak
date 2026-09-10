using ErrorOr;
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
    
    
}