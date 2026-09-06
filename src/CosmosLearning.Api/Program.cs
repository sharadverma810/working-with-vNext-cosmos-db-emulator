using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.ErrorHandling.Services;
using CosmosLearning.Api.Features.Products.Pagination;
using CosmosLearning.Api.Features.Products.Pagination.Telemetry;
using CosmosLearning.Api.Infrastructure.Cosmos;
using CosmosLearning.Api.Infrastructure.ErrorHandling;
using CosmosLearning.Api.Infrastructure.OpenTelemetry;
using CosmosLearning.Api.Infrastructure.Resilience;
using Microsoft.Azure.Cosmos;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Controllers
// ------------------------------------------------------------

builder.Services.AddControllers();

// ------------------------------------------------------------
// Swagger / OpenAPI
// ------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// Problem Details
// ------------------------------------------------------------

builder.Services.AddProblemDetails();

// ------------------------------------------------------------
// Global Exception Handling
// ------------------------------------------------------------

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ------------------------------------------------------------
// Cosmos DB Configuration
// ------------------------------------------------------------

builder.Services
    .AddOptions<CosmosOptions>()
    .Bind(builder.Configuration.GetSection(CosmosOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(
            options.Endpoint,
            UriKind.Absolute,
            out _),
        "Cosmos:Endpoint must be an absolute URI.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.AccountKey),
        "Cosmos:AccountKey must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.DatabaseName),
        "Cosmos:DatabaseName must be configured.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ContainerName),
        "Cosmos:ContainerName must be configured.")
    .ValidateOnStart();

// ------------------------------------------------------------
// Cosmos DB Client
// ------------------------------------------------------------

builder.Services.AddSingleton(sp =>
{
    var options = sp
        .GetRequiredService<
            Microsoft.Extensions.Options.IOptions<CosmosOptions>>()
        .Value;

    return new CosmosClient(
        options.Endpoint,
        options.AccountKey,
        new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,

            // Cosmos SDK built-in retry for 429 responses
            MaxRetryAttemptsOnRateLimitedRequests = 9,

            MaxRetryWaitTimeOnRateLimitedRequests =
                TimeSpan.FromSeconds(30)
        });
});



// ------------------------------------------------------------
// Register OpenTelemetry Sources
// ------------------------------------------------------------
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(
        resource =>
            resource.AddService(
                "CosmosLearning.Api"))
    .WithTracing(
        tracing =>
            tracing
                .AddAspNetCoreInstrumentation()

                .AddSource(
                    PaginationActivitySource.SourceName)
                // TEMPORARY:
                // Write traces to the console.
                .AddConsoleExporter()

                // ----------------------------------
                // OTLP Exporter
                //
                // Send traces to the OpenTelemetry
                // Collector running locally.
                // ----------------------------------

                .AddOtlpExporter(
                    options =>
                    {
                        options.Endpoint =
                            new Uri(
                                "http://localhost:4318");

                        options.Protocol =
                            OtlpExportProtocol.HttpProtobuf;
                    })
                )
    .WithMetrics(
        metrics =>
            metrics
                .AddAspNetCoreInstrumentation()

                .AddMeter(
                    PaginationMetrics.MeterName)
                // TEMPORARY:
                // Write metrics to the console.
                .AddConsoleExporter()
                //.AddPrometheusExporter()
                );


// ------------------------------------------------------------
// Health Checks
// ------------------------------------------------------------

builder.Services
    .AddHealthChecks()
    .AddCheck<CosmosHealthCheck>("cosmosdb");

// ------------------------------------------------------------
// Feature Services
// ------------------------------------------------------------

// Error Handling demonstration
builder.Services.AddScoped<ErrorHandlingDemoService>();

// Product Pagination feature
builder.Services.AddSingleton<ProductPaginationService>();

// ------------------------------------------------------------
// OpenTelemetry / Logging
// ------------------------------------------------------------
builder.Services.AddSingleton<PaginationTelemetry>();


builder.Services.AddApplicationOpenTelemetry(builder.Configuration);
// ------------------------------------------------------------

var app = builder.Build();

// ============================================================
// HTTP REQUEST PIPELINE
// ============================================================

// ------------------------------------------------------------
// Global Exception Handling
//
// Keep this early so it can handle exceptions from downstream
// middleware, controllers and services.
// ------------------------------------------------------------

app.UseExceptionHandler();

// ------------------------------------------------------------
// Correlation ID
// ------------------------------------------------------------

app.UseMiddleware<CorrelationIdMiddleware>();

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

// ------------------------------------------------------------
// Endpoints
// ------------------------------------------------------------

app.MapControllers();

app.MapHealthChecks("/health");
app.MapPrometheusScrapingEndpoint();

app.Run();