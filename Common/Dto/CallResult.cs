
using Common.Grpc;

namespace Common.Dto;

public record CallResult<T>(string Section,T? Data, AppError? Error);

public static class CallResultExtensions
{
    public static CallOutcome<T> AsOutcome<T>(this CallResult<T> result, bool required) =>
        new(result.Section, required, result.Data, result.Error);
}