using CosmosLearning.Api.Configuration;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
            ConnectionMode = ConnectionMode.Gateway
        });
});

var app = builder.Build();

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