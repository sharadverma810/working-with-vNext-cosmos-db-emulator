using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.ErrorHandling.Services;
using CosmosLearning.Api.Features.Products.Pagination;
using CosmosLearning.Api.Features.Products.Pagination.Telemetry;
using CosmosLearning.Api.Infrastructure.Cosmos;
using CosmosLearning.Api.Infrastructure.ErrorHandling;
using CosmosLearning.Api.Infrastructure.Resilience;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Controllers
// ============================================================

builder.Services.AddControllers();


// ============================================================
// Swagger / OpenAPI
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// ============================================================
// Problem Details
// ============================================================

builder.Services.AddProblemDetails();


// ============================================================
// Global Exception Handling
// ============================================================

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// ============================================================
// Cosmos DB Configuration
// ============================================================

builder.Services
    .AddOptions<CosmosOptions>()
    .Bind(
        builder.Configuration.GetSection(
            CosmosOptions.SectionName))
    .Validate(
        options =>
            Uri.TryCreate(
                options.Endpoint,
                UriKind.Absolute,
                out _),
        "Cosmos:Endpoint must be an absolute URI.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.AccountKey),
        "Cosmos:AccountKey must be configured.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.DatabaseName),
        "Cosmos:DatabaseName must be configured.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.ContainerName),
        "Cosmos:ContainerName must be configured.")
    .ValidateOnStart();


// ============================================================
// Cosmos DB Client
// ============================================================

builder.Services.AddSingleton(
    serviceProvider =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<CosmosOptions>>()
                .Value;

        return new CosmosClient(
            options.Endpoint,
            options.AccountKey,
            new CosmosClientOptions
            {
                ConnectionMode =
                    ConnectionMode.Gateway,

                // Cosmos SDK built-in retry for
                // HTTP 429 (rate limiting).
                MaxRetryAttemptsOnRateLimitedRequests =
                    9,

                MaxRetryWaitTimeOnRateLimitedRequests =
                    TimeSpan.FromSeconds(30)
            });
    });


// ============================================================
// OpenTelemetry
// ============================================================

const string serviceName =
    "WorkingWithVNextCosmosDbEmulator.Api";

builder.Services
    .AddOpenTelemetry()

    // --------------------------------------------------------
    // Resource
    // --------------------------------------------------------

    .ConfigureResource(
        resource =>
            resource.AddService(
                serviceName: serviceName))

    // --------------------------------------------------------
    // Tracing
    // --------------------------------------------------------

    .WithTracing(
        tracing =>
            tracing

                // ASP.NET Core incoming HTTP requests.
                .AddAspNetCoreInstrumentation()

                // HTTP client dependencies.
                .AddHttpClientInstrumentation()

                // Custom pagination activities.
                .AddSource(
                    PaginationActivitySource.SourceName)

                // Temporary local debugging.
                .AddConsoleExporter()

                // Send traces to the
                // OpenTelemetry Collector.
                .AddOtlpExporter(
                    options =>
                    {
                        options.Endpoint =
                            new Uri(
                                "http://localhost:4318/v1/traces");

                        options.Protocol =
                            OtlpExportProtocol.HttpProtobuf;
                    }))

    // --------------------------------------------------------
    // Metrics
    // --------------------------------------------------------

    .WithMetrics(
        metrics =>
            metrics

                // ASP.NET Core HTTP metrics.
                .AddAspNetCoreInstrumentation()

                // HTTP client metrics.
                .AddHttpClientInstrumentation()

                // Custom pagination metrics.
                .AddMeter(
                    PaginationMetrics.MeterName)

                // Temporary local debugging.
                .AddConsoleExporter()

                // Send metrics to the
                // OpenTelemetry Collector.
                .AddOtlpExporter(
                    (
                        exporterOptions,
                        metricReaderOptions) =>
                    {
                        exporterOptions.Endpoint =
                            new Uri(
                                "http://localhost:4318/v1/metrics");

                        exporterOptions.Protocol =
                            OtlpExportProtocol.HttpProtobuf;

                        metricReaderOptions
                            .PeriodicExportingMetricReaderOptions
                            .ExportIntervalMilliseconds =
                                5000;
                    }));


// ============================================================
// Health Checks
// ============================================================

builder.Services
    .AddHealthChecks()
    .AddCheck<CosmosHealthCheck>(
        "cosmosdb");


// ============================================================
// Feature Services
// ============================================================

// Error handling demonstration.
builder.Services.AddScoped<
    ErrorHandlingDemoService>();


// Product pagination feature.
builder.Services.AddSingleton<
    ProductPaginationService>();


// ============================================================
// Pagination Telemetry
// ============================================================

builder.Services.AddSingleton<
    PaginationTelemetry>();


var app = builder.Build();


// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================


// ------------------------------------------------------------
// Global Exception Handling
// ------------------------------------------------------------

app.UseExceptionHandler();


// ------------------------------------------------------------
// Correlation ID
// ------------------------------------------------------------

app.UseMiddleware<
    CorrelationIdMiddleware>();


// ------------------------------------------------------------
// Swagger
// ------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ------------------------------------------------------------
// HTTPS
// ------------------------------------------------------------

app.UseHttpsRedirection();


// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

app.UseAuthorization();


// ============================================================
// Endpoints
// ============================================================

app.MapControllers();

app.MapHealthChecks("/health");


// ============================================================
// Run Application
// ============================================================

app.Run();