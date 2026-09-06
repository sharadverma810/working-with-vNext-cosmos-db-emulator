using CosmosLearning.Api.Configuration;
using CosmosLearning.Api.Features.ErrorHandling.Services;
using CosmosLearning.Api.Features.Products.Pagination;
using CosmosLearning.Api.Infrastructure.ErrorHandling;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Swagger / OpenAPI
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

// Configure Cosmos DB options and validate them

builder.Services
    .AddOptions<CosmosOptions>()
    .Bind(builder.Configuration.GetSection(CosmosOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _),
        "Cosmos:Endpoint must be an absolute URI.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.AccountKey),
        "Cosmos:AccountKey must be configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName),
        "Cosmos:DatabaseName must be configured.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.ContainerName),
        "Cosmos:ContainerName must be configured.")
    .ValidateOnStart();

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
            MaxRetryAttemptsOnRateLimitedRequests = 9,
            MaxRetryWaitTimeOnRateLimitedRequests =
            TimeSpan.FromSeconds(30)
        });
});


// Register services for error handling demo and product pagination
builder.Services.AddScoped<ErrorHandlingDemoService>();
builder.Services.AddSingleton<ProductPaginationService>();

var app = builder.Build();


// ------------------------------------------------------------
// Exception Handling
// ------------------------------------------------------------

app.UseExceptionHandler();

// Configure Swagger for Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();