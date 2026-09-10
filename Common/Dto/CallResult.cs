
using Common.Grpc;

namespace Common.Dto;

public record CallResult<T>(T? Data, AppError? Error);

public static class CallResultExtensions
{
    public static CallOutcome<T> AsOutcome<T>(this CallResult<T> result, string section,bool required) =>
        new(section, required, result.Data, result.Error);
}