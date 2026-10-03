
using System.Diagnostics.CodeAnalysis;
using Common.Errors;
using Common.Grpc;

namespace Common.Dto;

public readonly record struct CallFailure(string Section, AppError Error);

public record CallResult(string Section, AppError? Error)
{
    public static CallFailure Fail(string section, AppError error) => new(section, error);
    public static CallResult Success(string section) => new(section, null);
    public static CallResult<T> Success<T>(string section, T data) => new(section, data, null);
    public bool IsSuccess => Error is null;

    public static implicit operator CallResult(CallFailure f) => new(f.Section, f.Error);
}

public record CallResult<T>(string Section, T? Data, AppError? Error)
{
    public CallFailure AsFailure() => new(Section, Error!);
    public static CallResult<T> Success(string section, T data) => new(section, data, null);
    [MemberNotNullWhen(true, nameof(Data))]
    public bool IsSuccess => Error is null;

    public static implicit operator CallResult<T>(CallFailure f) => new(f.Section, default, f.Error);
    
}

public static class CallResultExtensions
{
    public static CallOutcome<T> AsOutcome<T>(this CallResult<T> result, bool required) =>
        new(result.Section, required, result.Data, result.Error);

}
