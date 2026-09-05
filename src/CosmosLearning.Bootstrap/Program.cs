using CosmosLearning.Bootstrap;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;

var configuration = new ConfigurationBuilder()
	.SetBasePath(AppContext.BaseDirectory)
	.AddJsonFile("appsettings.json", optional: false)
	.AddEnvironmentVariables()
	.Build();

var options = configuration.GetSection("Cosmos").Get<BootstrapOptions>()
	?? throw new InvalidOperationException("The Cosmos configuration section is required.");

if (string.IsNullOrWhiteSpace(options.AccountKey))
{
	throw new InvalidOperationException("Cosmos:AccountKey must be supplied through configuration or the environment.");
}

if (options.DocumentCount <= 0 || options.ProgressInterval <= 0)
{
	throw new InvalidOperationException("Cosmos document count and progress interval must be greater than zero.");
}

using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
	eventArgs.Cancel = true;
	cancellationTokenSource.Cancel();
	Console.WriteLine("Cancellation requested. Finishing the current operation...");
};

try
{
	var reset = args.Any(argument => argument.Equals("reset", StringComparison.OrdinalIgnoreCase)
		|| argument.Equals("--reset", StringComparison.OrdinalIgnoreCase));
	await new BootstrapRunner(options, new ProductDataGenerator()).RunAsync(
		reset,
		cancellationTokenSource.Token);
}
catch (OperationCanceledException)
{
	Console.Error.WriteLine("Bootstrap was cancelled.");
	Environment.ExitCode = 2;
}
catch (CosmosException exception)
{
	Console.Error.WriteLine($"Bootstrap failed while preparing Cosmos DB: HTTP {(int)exception.StatusCode} {exception.StatusCode}");
	Environment.ExitCode = 1;
}
catch (InvalidOperationException exception)
{
	Console.Error.WriteLine($"Bootstrap failed: {exception.Message}");
	Environment.ExitCode = 1;
}
catch (HttpRequestException exception)
{
	Console.Error.WriteLine($"Bootstrap could not connect to Cosmos DB: {exception.Message}");
	Environment.ExitCode = 1;
}
