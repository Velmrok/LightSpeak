using System.Net;
using Common.Dto;
using Common.Grpc;
using Microsoft.AspNetCore.Http;

namespace Common.Services;

public interface IResponseBuilderService
{
    IResult BuildResponse<TData>(HttpStatusCode successStatusCode, IEnumerable<ICallOutcome> results, Func<TData> buildData);
    IResult BuildResponse<TResult,TResponse>(HttpStatusCode successStatusCode, CallResult<TResult> result, Func<TResponse> buildData);
    IResult BuildResponse(CallResult result);
    IResult BuildError(CallResult result);
    IResult BuildError<TData>(CallResult<TData> result);
}