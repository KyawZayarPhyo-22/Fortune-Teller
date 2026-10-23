using FortuneTeller.Domain.Features.AiReading;
using FortuneTeller.Domain.Features.ReadingOrchestrator;
using FortuneTeller.Domain.Features.WebSocketGateway;
using Microsoft.Extensions.DependencyInjection;

namespace FortuneTeller.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddSingleton<SpreadCardSelector>();
        services.AddSingleton<PalmImageAnalyzer>();
        services.AddSingleton<AiReadingService>();
        services.AddSingleton<ResponseFormatterService>();
        services.AddSingleton<WebSocketGatewayService>();
        services.AddSingleton<ReadingOrchestratorService>();

        return services;
    }
}
