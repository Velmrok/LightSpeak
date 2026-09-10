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
                Section,
                result,
                null);
        }
        catch (RpcException ex)
        {
            var appError = ex.ToAppError();

            return new CallResult<T>(
                Section,
                default,
                appError);
        }
    }
    
}