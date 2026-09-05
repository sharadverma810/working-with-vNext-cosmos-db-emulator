# Cosmos DB Emulator vNext Learning Project

This repository is being built as a local .NET 10 learning project for the
Azure Cosmos DB Emulator vNext. It will demonstrate the Cosmos DB API for
NoSQL, the Microsoft.Azure.Cosmos SDK, Product Catalog documents, CRUD,
point reads, parameterized queries, the actual HTTP `QUERY` method, and
continuation-token pagination.

The implementation is being developed incrementally. At the current
baseline, this repository contains the planning and emulator setup
documentation only. The .NET solution, API, bootstrap command, reset command,
and test project will be added by following the canonical plan:

[docs/cosmos-vnext-learning-plan.md](docs/cosmos-vnext-learning-plan.md)

Do not use commands for the application itself until the corresponding task
has been implemented and verified.

## Project Scope

The target local architecture is:

```text
Client
	|
	v
ASP.NET Core Web API
	|
	v
Microsoft.Azure.Cosmos SDK
	|
	v
Azure Cosmos DB Emulator vNext
	|
	v
Docker Desktop
```

The project is local-only. It does not require an Azure subscription and does
not currently cover Azure networking, private endpoints, MongoDB API
workloads, or production deployment.

## Prerequisites

- Windows
- Docker Desktop with the WSL 2 backend
- .NET 10 SDK
- Git
- PowerShell 5.1 or PowerShell 7+

Check the local tools:

```powershell
docker --version
docker info
wsl --status
dotnet --version
```

## Start the Emulator

Stop any older Windows Cosmos DB Emulator that is using port `8081`. Then
pull the official vNext image:

```powershell
docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest
docker volume create cosmos-vnext-data
```

Start the emulator in HTTPS mode:

```powershell
docker run -d `
	--name cosmos-vnext `
	-p 8081:8081 `
	-p 8080:8080 `
	-p 1234:1234 `
	-v cosmos-vnext-data:/data `
	mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-latest `
	--protocol https
```

Check status and logs:

```powershell
docker ps
docker logs cosmos-vnext --tail 50
curl.exe http://localhost:8080/ready
```

The readiness endpoint is HTTP on port `8080`. The Cosmos gateway is HTTPS
on port `8081`, and Data Explorer is normally available at
`https://localhost:1234`.

Healthy logs should include messages similar to:

```text
PostgreSQL=OK, Gateway=OK, Explorer=OK
System is now fully ready to accept requests
Now listening on: https://0.0.0.0:8081
```

For more emulator-specific troubleshooting, see
[Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md](Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md).

## Certificate Setup

The emulator uses a local HTTPS certificate. Download it with the method for
the PowerShell version in use, then import it into the current user's trusted
root store.

### Windows PowerShell 5.1

PowerShell 5.1 does not support `SkipCertificateCheck` on
`Invoke-WebRequest`. Use `curl.exe`:

```powershell
curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt
Import-Certificate `
	-FilePath .\emulatorcert.crt `
	-CertStoreLocation Cert:\CurrentUser\Root
```

### PowerShell 7+

```powershell
$parameters = @{
		Uri = 'https://localhost:8081/_explorer/emulator.pem'
		Method = 'GET'
		OutFile = 'emulatorcert.crt'
		SkipCertificateCheck = $True
}

Invoke-WebRequest @parameters
Import-Certificate `
	-FilePath .\emulatorcert.crt `
	-CertStoreLocation Cert:\CurrentUser\Root
```

The `-k` or `SkipCertificateCheck` option is used only while downloading the
local certificate. Do not disable certificate validation in the application.

## Current Repository Status

Task 1 is complete and the Task 2 solution/connectivity slice is implemented.
The repository now contains the .NET solution, API project, bootstrap project,
test project, typed Cosmos configuration, and one DI-managed `CosmosClient`.
It also contains the initial Product Catalog document model and its focused
serialization tests, deterministic sample-data generation, and the bootstrap
command, reset command, and CRUD API.

Configure the emulator account key through the environment rather than
committing it to source control:

```powershell
$env:Cosmos__AccountKey = '<emulator-account-key>'
dotnet run --project src/CosmosLearning.Api/CosmosLearning.Api.csproj --urls http://localhost:5099
```

The current connectivity endpoint is:

```text
GET http://localhost:5099/health/cosmos
```

The application performs a Cosmos account read and returns `200` only after a
successful SDK connection. Transport failures return `503` without exposing
secrets or stack traces.

Implemented and verified:

- `dotnet restore`
- `dotnet build CosmosLearning.slnx`
- `dotnet test`
- Emulator readiness and gateway availability
- Configuration validation and safe connectivity failure handling

Not yet implemented:

- advanced query scenarios

The next implementation step is Task 9 in
[docs/cosmos-vnext-learning-plan.md](docs/cosmos-vnext-learning-plan.md).

## HTTP QUERY Research

Task 7 research is complete in
[docs/http-query-research.md](docs/http-query-research.md). The disposable
.NET 10 probe confirmed that both Minimal API `MapMethods` and controller
`[AcceptVerbs("QUERY")]` can route the literal method. The recommended project
implementation remains controller-based and requires approval before Task 8.

The current ASP.NET Core 10 OpenAPI generator produces OpenAPI 3.1 and
excludes unknown methods such as `QUERY`. The literal method should therefore
be tested with `curl.exe` and `api.http`, not represented as `POST /query`.

## HTTP QUERY Endpoint

The actual endpoint is:

```text
QUERY /products/query
```

It accepts a structured JSON body with filters for category, subcategory,
status, price, rating, boolean flags, dates, warehouse city, and tags. It also
supports `sortField`, `sortDirection`, `pageSize`, and `continuationToken`.

Values are passed to Cosmos through `QueryDefinition` parameters. Sort fields
are restricted to `price`, `rating`, `createdAt`, and `productName`; direction
is restricted to `ASC` or `DESC`; page size is limited to 100. Raw Cosmos SQL
is not accepted.

Example:

```powershell
curl.exe -X QUERY http://localhost:5099/products/query `
	-H "Content-Type: application/json" `
	--data-binary "{...}"
```

See [scripts/sample-requests/api.http](scripts/sample-requests/api.http) for a
complete request body. The endpoint returns `items`, `count`,
`continuationToken`, `requestCharge`, and `activityId` where available.

### Advanced Query Scenarios

The QUERY builder and sample requests also demonstrate:

- Date ranges with `createdAfter` and `createdBefore`.
- Explicit JSON null values with `IS_NULL`.
- Missing properties with `IS_DEFINED`.
- Nested objects such as `manufacturer.country`, `metadata.source`, and
	`warehouseLocation.city`.
- Array membership with `ARRAY_CONTAINS`.
- Nested review arrays with `EXISTS`.
- A combined category, status, price, rating, active, and tag filter.

These scenarios remain parameterized; client values are never concatenated
into Cosmos SQL.

### QUERY Verification Status

Literal `QUERY` requests were sent to the running API. An invalid sort field
returned `400 Bad Request`, proving route and validation behavior. A valid
structured request reached Cosmos and returned the safe `503` transport error
because the emulator certificate still reports `UntrustedRoot`. Cosmos query
results and continuation tokens therefore remain unverified.

## Document Model and Partition Key

The Product Catalog model includes scalar values, nullable values, UTC dates,
arrays, nested objects, and nested reviews. Its serialization shape is covered
by automated tests.

The current container partition-key decision is `/category`. It supports the
main catalog learning scenario: category-filtered queries can target one
logical partition, while broader date, price, status, nested-object, and array
queries demonstrate cross-partition behavior. The alternatives and tradeoffs
are documented in
[docs/partition-key-decision.md](docs/partition-key-decision.md).

The application-level SDK connectivity check remains blocked on this machine:
the running emulator presents a self-signed non-root certificate and Windows
reports `UntrustedRoot` even after the downloaded certificate was imported.
Certificate validation remains enabled; no insecure bypass was added.

## Bootstrap Data

The bootstrap project generates 5,000 deterministic Product Catalog documents
with varied categories, statuses, prices, dates, booleans, tags, nested
objects, reviews, and nullable values. It creates only the configured
`LearningDb` database and `Products` container, using `/category` as the
partition key.

Supply the emulator key through the process environment:

```powershell
$env:Cosmos__AccountKey = '<emulator-account-key>'
dotnet run --project src/CosmosLearning.Bootstrap/CosmosLearning.Bootstrap.csproj
```

Optional configuration overrides use the same environment-variable convention,
for example:

```powershell
$env:Cosmos__DocumentCount = '5000'
$env:Cosmos__ProgressInterval = '500'
```

The command reports database/container readiness, insertion progress, total
generated documents, successful inserts, failed inserts, and duration. It
uses bounded sequential insertion and returns a nonzero exit code when a
failure occurs.

### Bootstrap Verification Status

The command was executed against the running emulator. It generated the
5,000-document dataset in memory, but the first Cosmos operation failed before
database creation because Windows rejected the emulator certificate with
`UntrustedRoot`. Therefore no documents were claimed as inserted and the
database/container count has not been verified.

## Reset Data

Reset the sample dataset with the same bootstrap project:

```powershell
$env:Cosmos__AccountKey = '<emulator-account-key>'
dotnet run --project src/CosmosLearning.Bootstrap/CosmosLearning.Bootstrap.csproj -- reset
```

The reset operation:

1. Connects to `LearningDb`.
2. Deletes only the configured `Products` container.
3. Recreates `Products` with partition key `/category`.
4. Generates and inserts the deterministic dataset again.

It does not delete the `LearningDb` database, other containers, or unrelated
emulator data. Running the command repeatedly is safe and idempotent once
Cosmos connectivity is available.

### Reset Verification Status

The reset command was executed, but the emulator certificate failed Windows
TLS validation before the database operation began. No deletion or recreation
was performed, and no reset success is claimed.

## Run the API

Start the API with the emulator account key supplied through the environment:

```powershell
$env:Cosmos__AccountKey = '<emulator-account-key>'
dotnet run --project src/CosmosLearning.Api/CosmosLearning.Api.csproj --urls http://localhost:5099
```

The Cosmos connectivity check is available at
`http://localhost:5099/health/cosmos`. The API uses the same configured
`LearningDb` database and `Products` container as the bootstrap command.

## CRUD API

CRUD request examples are in
[scripts/sample-requests/api.http](scripts/sample-requests/api.http).

| Operation | Method and route | Cosmos behavior |
| --- | --- | --- |
| Create | `POST /products` | Creates one document and returns `201 Created`. |
| List | `GET /products` | Uses a simple Cosmos query and returns items plus count. |
| Point read | `GET /products/{id}?category={category}` | Uses `ReadItemAsync` with id and `/category` partition key. |
| Replace | `PUT /products/{id}` | Replaces the complete document. |
| Delete | `DELETE /products/{id}?category={category}` | Deletes using id and partition key. |

Point reads and deletes require both the document id and its category because
`/category` is the container partition key. This is intentionally different
from the later structured QUERY endpoint.

The controller validates required identifiers, supports cancellation tokens,
maps common Cosmos errors to safe HTTP responses, and returns a `Retry-After`
header for throttling responses when Cosmos provides one. ETags, PATCH, and
the actual HTTP `QUERY` method are intentionally deferred to later tasks.

### CRUD Verification Status

The CRUD routes compile and their request samples pass editor diagnostics. Live
CRUD requests have not been verified because the local .NET Cosmos SDK cannot
complete TLS validation against the emulator's `UntrustedRoot` certificate.

## Planned Repository Structure

```text
src/
	CosmosLearning.Api/
	CosmosLearning.Bootstrap/
tests/
	CosmosLearning.Tests/
scripts/
	sample-requests/
docs/
	cosmos-vnext-learning-plan.md
```

## Useful Emulator Commands

```powershell
docker start cosmos-vnext
docker stop cosmos-vnext
docker restart cosmos-vnext
docker ps -a
docker logs cosmos-vnext --tail 50
curl.exe http://localhost:8080/ready
```

Removing the container does not remove the named data volume:

```powershell
docker rm -f cosmos-vnext
```

Do not remove `cosmos-vnext-data` unless deleting the emulator data is
intentional.

## Troubleshooting

### Port 8081 is already in use

```powershell
netstat -ano | findstr :8081
```

Stop the older Windows emulator or another process using the port. The
emulator's readiness endpoint is HTTP, so use:

```powershell
curl.exe http://localhost:8080/ready
```

Do not use HTTPS on port `8080`.

### Container name already exists

Inspect existing containers:

```powershell
docker ps -a
```

Remove the old container only when recreation is intentional:

```powershell
docker rm -f cosmos-vnext
```

The named volume remains unless it is explicitly removed.

### Certificate errors

Confirm that `emulatorcert.crt` was imported into
`Cert:\CurrentUser\Root`. Use the PowerShell 5.1 instructions if
`SkipCertificateCheck` is reported as an unknown parameter.

### The emulator is not ready

Run:

```powershell
docker ps
docker logs cosmos-vnext --tail 50
curl.exe http://localhost:8080/ready
```

Wait for the readiness response and healthy gateway/explorer log messages
before beginning application work.

## Verification Status

| Check | Status |
| --- | --- |
| .NET 10 SDK available | Successfully verified |
| Docker and WSL 2 available | Successfully verified |
| Emulator readiness | Successfully verified |
| Solution restore and build | Successfully verified |
| Automated test baseline | Successfully verified |
| Product Catalog serialization tests | Successfully verified |
| Partition-key analysis | Successfully verified |
| Deterministic generator tests | Successfully verified |
| Application-level Cosmos SDK connectivity | Blocked by emulator certificate trust |
| Bootstrap execution against emulator | Blocked by emulator certificate trust |
| Reset implementation | Implemented; execution blocked by emulator certificate trust |
| CRUD implementation | Implemented; live execution blocked by emulator certificate trust |
| QUERY research | Successfully verified |
| QUERY routing, validation, and parameterized builder | Successfully verified |
| Advanced query construction and examples | Successfully verified |
| QUERY Cosmos execution and pagination | Blocked by emulator certificate trust |

The final README will expand this table as each task is implemented and
verified with real commands.
