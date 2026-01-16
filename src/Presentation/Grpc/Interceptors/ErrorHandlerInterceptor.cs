using Grpc.Core;
using Grpc.Core.Interceptors;
using System.Security.Authentication;

namespace Presentation.Grpc.Interceptors;

public class ErrorHandlerInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw ToRpcException(ex);
        }
    }

    private static RpcException ToRpcException(Exception ex)
    {
        StatusCode code = ex switch
        {
            ArgumentException or FormatException => StatusCode.InvalidArgument,
            KeyNotFoundException => StatusCode.NotFound,
            FileNotFoundException => StatusCode.NotFound,
            InvalidOperationException => StatusCode.FailedPrecondition,
            OperationCanceledException => StatusCode.Cancelled,
            UnauthorizedAccessException => StatusCode.PermissionDenied,
            AuthenticationException => StatusCode.Unauthenticated,
            InsufficientMemoryException => StatusCode.ResourceExhausted,
            NotSupportedException => StatusCode.Unimplemented,
            HttpRequestException => StatusCode.Unavailable,
            _ => StatusCode.Internal,
        };

        return new RpcException(new Status(code, ex.Message));
    }
}