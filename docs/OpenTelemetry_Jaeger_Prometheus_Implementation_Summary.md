# OpenTelemetry + Jaeger + Prometheus Setup

## Project
**WorkingWithVNextCosmosDbEmulator.Api**

This session added end-to-end observability for the .NET 10 API using:

- OpenTelemetry
- Custom pagination traces and metrics
- OpenTelemetry Collector
- Jaeger for distributed tracing
- Prometheus for metrics
- Cosmos DB Emulator

---

## 1. Working Architecture

```text
.NET 10 API
    |
    | OTLP HTTP
    | localhost:4318
    v
OpenTelemetry Collector
    |                         |
    | Traces                  | Metrics
    v                         v
Jaeger                    Prometheus
:16686                    :9090
```

The API sends telemetry to the collector:

- Traces: `http://localhost:4318/v1/traces`
- Metrics: `http://localhost:4318/v1/metrics`

The collector:

- exports traces to Jaeger
- exposes metrics for Prometheus scraping

---

## 2. Custom Pagination Observability

The pagination feature includes:

### Custom ActivitySource

```csharp
PaginationActivitySource.SourceName
```

Used to create custom spans such as:

```text
ProductPagination.GetPage
ProductPagination.Service.GetPage
ProductPagination.QueryBuilder.Build
ProductPagination.Cosmos.ReadNext
```

### Custom Meter

```csharp
PaginationMetrics.MeterName
```

Pagination telemetry includes useful dimensions such as:

- page size
- continuation token present
- category filter
- active filter
- minimum price filter
- maximum price filter
- returned item count
- Cosmos request charge
- pagination duration

---

## 3. OpenTelemetry Configuration

The final working `Program.cs` configuration includes:

### Resource

```csharp
.ConfigureResource(resource =>
    resource.AddService(
        "WorkingWithVNextCosmosDbEmulator.Api"))
```

### Tracing

```csharp
.WithTracing(tracing => tracing
    .AddAspNetCoreInstrumentation()
    .AddSource(PaginationActivitySource.SourceName)
    .AddConsoleExporter()
    .AddOtlpExporter(options =>
    {
        options.Endpoint =
            new Uri("http://localhost:4318/v1/traces");

        options.Protocol =
            OtlpExportProtocol.HttpProtobuf;
    }))
```

### Metrics

```csharp
.WithMetrics(metrics => metrics
    .AddAspNetCoreInstrumentation()
    .AddMeter(PaginationMetrics.MeterName)
    .AddConsoleExporter()
    .AddOtlpExporter(
        (exporterOptions, metricReaderOptions) =>
        {
            exporterOptions.Endpoint =
                new Uri(
                    "http://localhost:4318/v1/metrics");

            exporterOptions.Protocol =
                OtlpExportProtocol.HttpProtobuf;

            metricReaderOptions
                .PeriodicExportingMetricReaderOptions
                .ExportIntervalMilliseconds = 5000;
        }))
```

Console exporters were useful during debugging to verify that telemetry was being generated.

---

## 4. Important Tracing Improvement

The working tracing configuration includes:

```csharp
.AddHttpClientInstrumentation()
```

This is important because Cosmos DB Gateway mode generates HTTP calls.

With HTTP client instrumentation enabled, traces can show Cosmos DB calls such as:

```text
DELETE https://localhost:8081/...
GET/POST Cosmos operations
```

This gives better visibility into the dependency calls made by the API.

---

## 5. OpenTelemetry Collector Configuration

The collector receives OTLP telemetry:

```yaml
receivers:

  otlp:

    protocols:

      grpc:
        endpoint: 0.0.0.0:4317

      http:
        endpoint: 0.0.0.0:4318
```

### Batch Processor

```yaml
processors:

  batch:
```

### Jaeger Exporter

```yaml
exporters:

  otlp/jaeger:

    endpoint: jaeger:4317

    tls:
      insecure: true
```

### Prometheus Exporter

```yaml
  prometheus:

    endpoint: 0.0.0.0:8889
```

### Pipelines

```yaml
service:

  pipelines:

    traces:

      receivers:
        - otlp

      processors:
        - batch

      exporters:
        - otlp/jaeger

    metrics:

      receivers:
        - otlp

      processors:
        - batch

      exporters:
        - prometheus
```

---

## 6. Pagination Endpoint Test

The working pagination endpoint:

```http
GET {{host}}/products/pages?pageSize=5
Accept: application/json
```

Continuation tokens can be passed using:

```http
x-continuation-token: <token>
```

The pagination endpoint successfully generated:

- HTTP request traces
- custom pagination traces
- pagination metrics
- Cosmos request charge information

---

## 7. Jaeger Verification

Jaeger successfully displayed API traces.

A typical trace included:

```text
GET products/pages
    |
    +-- ProductPagination.GetPage
            |
            +-- ProductPagination.Service.GetPage
                    |
                    +-- ProductPagination.QueryBuilder.Build
                    |
                    +-- ProductPagination.Cosmos.ReadNext
```

This confirmed that distributed tracing from the API to the OpenTelemetry Collector and then Jaeger was working.

---

## 8. Prometheus Setup

Prometheus successfully discovered the OpenTelemetry Collector target:

```text
otel-collector:8889/metrics
```

Target health:

```text
UP
```

The Prometheus URL is:

```text
http://localhost:9090
```

The collector metrics endpoint is:

```text
http://localhost:8889/metrics
```

---

## 9. Important Debugging Lessons

### `target_info` returned no data

This did not necessarily mean the Prometheus setup was broken.

The more important verification was:

1. Prometheus target is `UP`.
2. The collector receives metrics.
3. The collector exposes metrics on port `8889`.
4. The API generates metric instruments.

### Console exporter was valuable

The OpenTelemetry console exporter confirmed that:

- ASP.NET Core spans were generated.
- custom pagination activities were generated.
- HTTP dependency spans were generated.

Example:

```text
InstrumentationScope Microsoft.AspNetCore
Name: GET products
Kind: Server
http.response.status_code: 200
```

### Collector debug logs

The collector showed traces arriving successfully. This was useful to confirm:

```text
API -> Collector
```

before checking:

```text
Collector -> Jaeger
Collector -> Prometheus
```

---

## 10. Final Result

The application now has working observability across the complete telemetry pipeline.

### Traces

```text
.NET API
    -> OpenTelemetry Collector
        -> Jaeger
```

### Metrics

```text
.NET API
    -> OpenTelemetry Collector
        -> Prometheus
```

### Cosmos DB

The API continues to use the local Cosmos DB Emulator and the pagination feature provides:

- continuation token support
- filtered reads
- custom tracing
- custom metrics
- Cosmos request charge telemetry
- duration telemetry

---

## 11. Current Status

| Capability | Status |
|---|---|
| .NET 10 API | Working |
| Cosmos DB Emulator | Working |
| Product Pagination | Working |
| Continuation Tokens | Working |
| Custom ActivitySource | Working |
| Custom Meter | Working |
| OpenTelemetry Collector | Working |
| Jaeger Traces | Working |
| Prometheus Target Discovery | Working |
| Console Telemetry | Working |

---

## 12. Suggested Next Steps

The next logical observability improvements are:

1. Verify and query the custom application metrics in Prometheus.
2. Create a Grafana dashboard.
3. Add Cosmos DB-specific metrics where available.
4. Add resilience telemetry for retries and failures.
5. Correlate logs, traces, and metrics.
6. Remove temporary console exporters after the telemetry pipeline is fully validated.

---

## Key Takeaway

The most important result from this session is that observability is no longer limited to application logs.

The API now provides:

```text
Request
   |
   +-- Trace
   |
   +-- Custom Pagination Span
   |
   +-- Cosmos Dependency
   |
   +-- Metrics
```

This provides a strong foundation for the next phases of the vNext Cosmos DB Emulator project.
