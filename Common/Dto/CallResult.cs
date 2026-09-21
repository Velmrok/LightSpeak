
using System.Diagnostics.CodeAnalysis;
using Common.Errors;
using Common.Grpc;

namespace Common.Dto;

public record CallResult<T>(string Section,T? Data, AppError? Error)
{
     public CallResult<TTarget> Fail<TTarget>() =>new(Section, default, Error);
     public static CallResult<T> Fail(string section, AppError error) => new(section, default, error);
     public static CallResult<T> Success(string section, T data) => new(section, data, null);
     public static CallResult<Empty> Success(string section) => new(section, new Empty(), null);
}

public static class CallResultExtensions
{
    public static CallOutcome<T> AsOutcome<T>(this CallResult<T> result, bool required) =>
        new(result.Section, required, result.Data, result.Error);

    public static bool IsSuccess<T>(this CallResult<T> result) => result.Error is null;
    

}