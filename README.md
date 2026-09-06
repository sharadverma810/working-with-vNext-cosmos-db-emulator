# Working with vNext Cosmos DB Emulator (Learning Project)

This repository is a local .NET 10 learning project that demonstrates using the
Azure Cosmos DB Emulator vNext with a small ASP.NET Core Web API, a
bootstrap/seeding tool, and OpenTelemetry for observability.

Top-level layout
- src/CosmosLearning.Api       - ASP.NET Core Web API (products, pagination, telemetry)
- src/CosmosLearning.Bootstrap - Console app to seed/reset the Cosmos DB container
- tests/CosmosLearning.Tests   - Unit tests
- docker-compose.yml          - Optional observability stack (Jaeger, OTEL Collector, Prometheus)
- docs/                       - Detailed guides, including the emulator setup

Prerequisites
- .NET 10 SDK
- Docker Desktop (with WSL2 on Windows) — required to run the vNext emulator and the optional observability stack
- PowerShell (Windows) or a POSIX shell

Quick start (minimal)

1. Restore and build

   - dotnet restore "CosmosLearning.slnx"
   - dotnet build "CosmosLearning.slnx"

2. Start the optional observability stack (recommended)

   From the repository root:
   - docker compose up -d

   UIs:
   - Jaeger:    http://localhost:16686
   - Prometheus: http://localhost:9090

3. Run the Cosmos DB vNext emulator

   - Follow the detailed guide in docs/Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md to run the emulator and import the local certificate into the current user's trusted root store.

   - Default bootstrap/appsettings expect the gateway at: https://localhost:8081/ (see src/CosmosLearning.Bootstrap/appsettings.json).

4. Seed the database (Bootstrap)

   - Provide the emulator account key via environment variable (do not commit it):
	 PowerShell example:
	   $env:COSMOS__ACCOUNTKEY = '<emulator-account-key>'

   - Run the bootstrap tool (reset will recreate data):
	   dotnet run --project src/CosmosLearning.Bootstrap -- --reset

5. Run the Web API

   - From Visual Studio: open CosmosLearning.slnx and run the CosmosLearning.Api project.
   - From the command line:
	   dotnet run --project src/CosmosLearning.Api

   - The API is instrumented with OpenTelemetry and configured to export to the local OTEL collector at http://localhost:4318 by default.

6. Run tests

   - dotnet test tests/CosmosLearning.Tests

Configuration
- The apps use a configuration section named "Cosmos" with these required keys:
  - Cosmos:Endpoint
  - Cosmos:AccountKey
  - Cosmos:DatabaseName
  - Cosmos:ContainerName

You can set these in appsettings.json or via environment variables using the double-underscore convention (example: COSMOS__ACCOUNTKEY).

Observability
- OTEL Collector endpoint (configured in src/CosmosLearning.Api/Program.cs): http://localhost:4318
- Jaeger UI: http://localhost:16686
- Prometheus: http://localhost:9090

API highlights
- The project exposes a health endpoint that verifies Cosmos connectivity: /health/cosmos
- There is a QUERY-based endpoint for advanced searches: QUERY /products/query (examples in scripts/sample-requests/api.http)

Troubleshooting & notes
- Certificate errors: import the emulator certificate into Cert:\CurrentUser\Root (see docs for PowerShell 5.1 vs 7+ instructions).
- If port 8081 is occupied, stop the older Windows emulator or the conflicting process.
- The bootstrap app will fail if Cosmos:AccountKey is missing — supply it via environment variable for local runs.

References
- docs/Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md — step-by-step emulator guide
- docs/http-query-research.md — notes about the HTTP QUERY method and routing

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
