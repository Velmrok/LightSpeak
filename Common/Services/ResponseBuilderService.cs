
using System.Net;
using Common.Dto;
using Common.Grpc;
using Common.Mappers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;

namespace Common.Services;

public class ResponseBuilderService : IResponseBuilderService 
{
    public IResult BuildResponse<TData>(HttpStatusCode successStatusCode, IEnumerable<ICallOutcome> results, Func<TData> buildData)
    {
        if (TryBuildError(results, out var errorResponse))
            return errorResponse!;

        return BuildSuccessResponse(successStatusCode, buildData(), results);
    }
    public IResult BuildResponse<TData>(HttpStatusCode successStatusCode, CallResult<TData> result, Func<TData> buildData)
    {
        var outcome = result.AsOutcome(required: true);
        if (TryBuildError([outcome], out var errorResponse))
            return errorResponse!;

        return BuildSuccessResponse(successStatusCode, buildData(), [outcome]);
    }
    private bool TryBuildError(IEnumerable<ICallOutcome> results, out IResult? response)
    {
        var requiredFailed = results.Where(r => r.Required && r.Error != null).ToList();
        if (requiredFailed.Any())
        {
            var top = requiredFailed
                //.OrderByDescending(e => e.ToRestError().Priority) No ordering for now, just take the first one
                .First();
            ErrorResponse data = new(
                Code: top.Error!.Code,
                Details: top.Error.Details,
                Errors: results.Where(r => r.Error != null).Select(e => new ErrorItem(e.Section, e.Error!.Code, e.Error.Details))
            );
            response = Results.Json(data, statusCode: top.Error.StatusCode.ToHttp());
            return true;
        }
        response = null;
        return false;
    }
    private IResult BuildSuccessResponse<TData>(HttpStatusCode statusCode, TData data, IEnumerable<ICallOutcome> results)
    {
        var optionalFailed = results.Where(r => !r.Required && r.Error != null).ToList();
        var optionalErrors = optionalFailed.Select(e => new ErrorItem(e.Section, e.Error!.Code, e.Error.Details)).ToList();
        var response = new ApiResponse<TData>(Data: data, Errors: optionalErrors);
        return Results.Json(response, statusCode: (int)statusCode);
    }
}