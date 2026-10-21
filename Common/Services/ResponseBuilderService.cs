
using System.Net;
using Common.Dto;
using Common.Grpc;
using Common.Mappers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Common.Services;

public class ResponseBuilderService : IResponseBuilderService
{
    public IResult BuildResponse<TData>(
        HttpStatusCode successStatusCode,
        IEnumerable<ICallOutcome> results,
        Func<TData> buildData)
    {
        var list = results.ToList();
        return TryBuildError(list) ?? BuildSuccess(successStatusCode, buildData(), list);
    }

    public IResult BuildResponse<TResult,TResponse>(
        HttpStatusCode successStatusCode,
        CallResult<TResult> result,
        Func<TResponse> buildData)
        => BuildResponse(successStatusCode, [result.AsOutcome(required: true)], buildData);

    public IResult BuildResponse(CallResult result)
    {
        ICallOutcome[] outcomes = [result.AsOutcome(required: true)];
        return TryBuildError(outcomes) ?? Results.NoContent();
    }
    public IResult BuildError<TData>(CallResult<TData> result)
        => TryBuildError([result.AsOutcome(required: true)])
           ?? throw new ArgumentException("Result must have an error", nameof(result));
    public IResult BuildError(CallResult result)
        => TryBuildError([result.AsOutcome(required: true)])
           ?? throw new ArgumentException("Result must have an error", nameof(result));

    private static IResult? TryBuildError(IReadOnlyCollection<ICallOutcome> results)
    {
        var top = results.FirstOrDefault(r => r is { Required: true, Error: not null });
        if (top is null)
            return null;

        var error = top.Error!;
        var data = new ErrorResponse(
            Code: error.Code,
            Details: error.Details,
            Errors: ToErrorItems(results.Where(r => r.Error != null)));

        return Results.Json(data, statusCode: error.StatusCode.ToHttp());
    }

    private static IResult BuildSuccess<TData>(
        HttpStatusCode statusCode, TData data, IReadOnlyCollection<ICallOutcome> results)
        => Results.Json(new ApiResponse<TData>(data, OptionalErrors(results)), statusCode: (int)statusCode);

    

    private static List<ErrorItem> OptionalErrors(IEnumerable<ICallOutcome> results)
        => ToErrorItems(results.Where(r => !r.Required && r.Error != null));

    private static List<ErrorItem> ToErrorItems(IEnumerable<ICallOutcome> failed)
        => [.. failed.Select(e => new ErrorItem(e.Section, e.Error!.Code, e.Error.Details))];
}