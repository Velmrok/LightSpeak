using System.Net;
using Common.Dto;
using Common.Grpc;
using Microsoft.AspNetCore.Http;

namespace Common.Services;

public interface IResponseBuilderService
{
    IResult BuildResponse<TData>(HttpStatusCode successStatusCode, IEnumerable<ICallOutcome> results, Func<TData> buildData);
    IResult BuildResponse<TData>(HttpStatusCode successStatusCode, CallResult<TData> result, Func<TData> buildData);
}