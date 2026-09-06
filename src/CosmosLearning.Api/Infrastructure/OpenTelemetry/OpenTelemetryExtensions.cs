namespace CosmosLearning.Api.Infrastructure.OpenTelemetry;

using CosmosLearning.Api.Features.Products.Pagination.Telemetry;
using global::OpenTelemetry.Metrics;
using global::OpenTelemetry.Resources;
using global::OpenTelemetry.Trace;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddApplicationOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        const string serviceName = "WorkingWithVNextCosmosDbEmulator.Api";

        services
            .AddOpenTelemetry()
            .ConfigureResource(
                resource =>
                    resource.AddService(
                        serviceName: serviceName))
            .WithTracing(
                tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()

                        .AddHttpClientInstrumentation()

                        .AddSource(
                            PaginationActivitySource.SourceName)

                        .AddOtlpExporter();
                })
            .WithMetrics(
                metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()

                        .AddHttpClientInstrumentation()

                        .AddMeter(
                            PaginationMetrics.MeterName)

                        .AddPrometheusExporter();
                });

        return services;
    }
}
