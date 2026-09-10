using Common.Dto;
using Common.Mappers;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Grpc;

public class GrpcCallHandler
{
    public async Task<CallResult<T>> SafeCall<T>( string Section, Func<DateTime, CancellationToken, Task<T>> call, CancellationToken cancellationToken)
    {
        try
        {
            var deadline = DateTime.UtcNow.AddSeconds(10);
            var result = await call(deadline, cancellationToken);

            return new CallResult<T>(
                result,
                null);
        }
        catch (RpcException ex)
        {
            var appError = ex.ToAppError();

            return new CallResult<T>(
                default,
                appError);
        }
    }
    public IResult BuildResponse<TData>(IEnumerable<ICallOutcome> results, Func<TData> buildData)
    {
        if (TryBuildError(results, out var errorResponse))
            return errorResponse!;

        return BuildSuccessResponse(buildData(), results); 
    }
    public IResult BuildResponse<TData>(ICallOutcome result, Func<TData> buildData)
    {
        if (TryBuildError([result], out var errorResponse))
            return errorResponse!;

        return BuildSuccessResponse(buildData(), [result]); 
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
                Errors: results.Where(r=>r.Error != null).Select(e => new ErrorItem(e.Section, e.Error!.Code, e.Error.Details))
            );
            response = top.Error.StatusCode.ToRestResponse(data);
            return true;
        }
        response = null;
        return false;
    }
    private IResult BuildSuccessResponse<TData>(TData data, IEnumerable<ICallOutcome> results)
    {
        var optionalFailed = results.Where(r => !r.Required && r.Error != null).ToList();
        var optionalErrors = optionalFailed.Select(e => new ErrorItem(e.Section, e.Error!.Code, e.Error.Details)).ToList();
        var response = new ApiResponse<TData>(Data: data, Errors: optionalErrors);
        return StatusCode.OK.ToRestResponse(response);
    }
}