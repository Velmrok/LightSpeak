using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;
namespace Common; 
public class JwtInterceptor(HttpContextAccessor httpContextAccessor) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var token = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(token))
        {
            return continuation(request, context);
        }
        var headers = context.Options.Headers ?? [];

        headers.Add("Authorization",token);

        var options = context.Options.WithHeaders(headers);

        return continuation(
            request,
            new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                options));
    }
}