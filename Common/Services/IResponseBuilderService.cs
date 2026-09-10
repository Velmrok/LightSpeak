using Common.Dto;
using Common.Grpc;
using Microsoft.AspNetCore.Http;

namespace Common.Services;

public interface IResponseBuilderService
{
    IResult BuildResponse<TData>(IEnumerable<ICallOutcome> results, Func<TData> buildData);
    IResult BuildResponse<TData>(ICallOutcome result, Func<TData> buildData);
}