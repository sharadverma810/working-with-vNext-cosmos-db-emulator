using CosmosLearning.Vector.Api.Features.VectorSearch;
using CosmosLearning.Vector.Api.Infrastructure.CosmosDb;
using CosmosLearning.Vector.Api.Infrastructure.Ollama;
using CosmosLearning.Vector.Api.Options;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Options
// -------------------------------------------------------

builder.Services.Configure<CosmosOptions>(builder.Configuration.GetSection(CosmosOptions.SectionName));

builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection(OllamaOptions.SectionName));



// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// -------------------------------------------------------
// Ollama HTTP Client
// -------------------------------------------------------

builder.Services.AddHttpClient<IOllamaEmbeddingService,
        OllamaEmbeddingService>(
        client =>
        {
            string? baseUrl =
                builder.Configuration["Ollama:BaseUrl"];

            client.BaseAddress =
                new Uri(baseUrl!);

            client.Timeout =
                TimeSpan.FromMinutes(2);
        });
// -------------------------------------------------------
// Cosmos DB
// -------------------------------------------------------

builder.Services.AddSingleton<CosmosClient>(
    serviceProvider =>
    {
        IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();

        string endpoint = configuration["Cosmos:Endpoint"]!;

        string key = configuration["Cosmos:Key"]!;

        var options = new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway,

            HttpClientFactory = () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler
                            .DangerousAcceptAnyServerCertificateValidator
                };

                return new HttpClient(handler);
            }
        };

        return new CosmosClient(endpoint, key, options);
    });


// -------------------------------------------------------
// Application Services
// -------------------------------------------------------

builder.Services.AddSingleton<CosmosVectorDatabaseInitializer>();

builder.Services.AddSingleton<CosmosVectorRepository>();

builder.Services.AddScoped<VectorSearchService>();


var app = builder.Build();


// -------------------------------------------------------
// Initialize Cosmos DB
// -------------------------------------------------------

try
{
    using IServiceScope scope =
        app.Services.CreateScope();

    var initializer =
        scope.ServiceProvider.GetRequiredService<
            CosmosVectorDatabaseInitializer>();

    await initializer.InitializeAsync();
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("==============================================");
    Console.WriteLine("COSMOS VECTOR DATABASE INITIALIZATION FAILED");
    Console.WriteLine("==============================================");
    Console.WriteLine($"Exception Type : {ex.GetType().FullName}");
    Console.WriteLine($"Message        : {ex.Message}");
    Console.WriteLine($"Stack Trace    : {ex.StackTrace}");
    Console.WriteLine("==============================================");
    Console.WriteLine();

    throw;
}




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
