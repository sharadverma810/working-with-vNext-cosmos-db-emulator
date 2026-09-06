using CosmosLearning.Api.Features.Products.Pagination.Telemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CosmosLearning.Api.Infrastructure.OpenTelemetry;

public static class OpenTelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        const string serviceName =
            "WorkingWithVNextCosmosDbEmulator.Api";

        services
            .AddOpenTelemetry()

            // ------------------------------------------------
            // Resource
            // ------------------------------------------------

            .ConfigureResource(
                resource =>
                    resource.AddService(
                        serviceName: serviceName))

            // ------------------------------------------------
            // Tracing
            // ------------------------------------------------

            .WithTracing(
                tracing =>
                {
                    tracing

                        // Incoming ASP.NET Core requests.
                        .AddAspNetCoreInstrumentation()

                        // Outgoing HTTP requests.
                        .AddHttpClientInstrumentation()

                        // Custom pagination ActivitySource.
                        .AddSource(
                            PaginationActivitySource.SourceName)

                        // Send traces to the Collector.
                        .AddOtlpExporter(
                            options =>
                            {
                                options.Endpoint =
                                    new Uri(
                                        "http://localhost:4318/v1/traces");

                                options.Protocol =
                                    OtlpExportProtocol.HttpProtobuf;
                            });
                })

            // ------------------------------------------------
            // Metrics
            // ------------------------------------------------

            .WithMetrics(
                metrics =>
                {
                    metrics

                        // ASP.NET Core metrics.
                        .AddAspNetCoreInstrumentation()

                        // HTTP client metrics.
                        .AddHttpClientInstrumentation()

                        // Custom pagination metrics.
                        .AddMeter(
                            PaginationMetrics.MeterName)

                        // Send metrics to the Collector.
                        .AddOtlpExporter(
                            (exporterOptions, metricReaderOptions) =>
                            {
                                exporterOptions.Endpoint =
                                    new Uri(
                                        "http://localhost:4318/v1/metrics");

                                exporterOptions.Protocol =
                                    OtlpExportProtocol.HttpProtobuf;

                                // Development-friendly interval.
                                metricReaderOptions
                                    .PeriodicExportingMetricReaderOptions
                                    .ExportIntervalMilliseconds =
                                        5000;
                            });
                });

        return services;
    }
}