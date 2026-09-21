namespace Common.Errors;

public enum DomainErrorCode
{
    Validation,
    BusinessRule,
    Unauthenticated,
    Forbidden,
    NotFound,
    Conflict,
    RateLimited,
    Cancelled,
    NotImplemented,
    Unavailable,
    Timeout,
    Internal
}