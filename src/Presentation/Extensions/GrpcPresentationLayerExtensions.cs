using AccountMaster.Grpc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Grpc.Interceptors;
using Presentation.Grpc.Service;

namespace Presentation.Extensions;

public static class GrpcPresentationLayerExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ErrorHandlerInterceptor>();
        services.AddGrpc(options => options.Interceptors.Add<ErrorHandlerInterceptor>());
        services.AddScoped<GrpcPassengerService>();

        string accountServiceAddress =
            configuration.GetValue<string>("Grpc:AccountServiceAddress")
            ?? throw new InvalidOperationException("Grpc:AccountServiceAddress is not configured");

        services.AddGrpcClient<AccountService.AccountServiceClient>(options =>
        {
            options.Address = new Uri(accountServiceAddress);
        });

        return services;
    }
}