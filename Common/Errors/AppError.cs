using Grpc.Core;

namespace Common.Errors;


public class AppError{
    public DomainErrorCode StatusCode { get; init; }
    public string Code { get; init; }
    public string Details { get; init; }
    public const string ErrorTitleKey = "error-code";
    public AppError(DomainErrorCode statusCode, string code, string details)
    {                       
        StatusCode = statusCode;
        Code = code;
        Details = details;
    }
    
    
}