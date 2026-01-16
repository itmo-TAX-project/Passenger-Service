using Microsoft.Extensions.DependencyInjection;
using Presentation.Grpc.Interceptors;
using Presentation.Grpc.Service;

namespace Presentation.Extensions;

public static class GrpcPresentationLayerExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddSingleton<ErrorHandlerInterceptor>();
        services.AddGrpc(options => options.Interceptors.Add<ErrorHandlerInterceptor>());
        services.AddScoped<GrpcPassengerService>();

        return services;
    }
}