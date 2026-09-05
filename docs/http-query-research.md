# HTTP QUERY Research Report

## Status

Task 7 research is complete. No application code was modified for this task.
Task 8 remains an approval gate: do not implement the QUERY endpoint until this report is reviewed.

## Installed Versions

Verified on 5 September 2026:

| Component | Version or state |
| --- | --- |
| Operating system | Windows 10 build 10.0.26200 |
| .NET SDK | 10.0.400 |
| .NET runtime | 10.0.11 |
| ASP.NET Core runtime | 10.0.11 |
| API framework | ASP.NET Core controller-based Web API |
| Cosmos SDK | Microsoft.Azure.Cosmos 3.62.1 |
| JSON dependency | Newtonsoft.Json 13.0.4 |
| Test SDK | Microsoft.NET.Test.Sdk 17.14.1 |
| xUnit | 2.9.3 |
| Swagger/OpenAPI package | Not installed in the repository |

The API currently uses `AddControllers()` and `MapControllers()`. It does not
currently register Swagger, Swashbuckle, or `Microsoft.AspNetCore.OpenApi`.

## HTTP QUERY Support

HTTP method names are tokens and HTTP supports extension methods. RFC 9110
requires servers to return `501 Not Implemented` for an unrecognized method and
allows additional methods to be defined by extensions. The current IETF QUERY
specification is a draft, not an Internet Standard RFC. It defines QUERY as a
safe, idempotent method whose request content describes the query.

ASP.NET Core 10 supports custom method routing:

- Minimal APIs expose `MapMethods(pattern, httpMethods, handler)`.
- MVC exposes the concrete `AcceptVerbsAttribute`, whose constructor accepts a
  string method name such as `QUERY`.
- `HttpMethodAttribute` is the underlying abstract base class. It is not applied
  directly; a custom derived attribute or `AcceptVerbs("QUERY")` must be used.

## Local Probe Result

A disposable .NET 10 probe was created outside this repository. It compiled and
accepted literal requests to both mechanisms:

```csharp
app.MapMethods("/minimal-query", ["QUERY"], () => Results.Ok("minimal-query"));

[AcceptVerbs("QUERY")]
public IActionResult Query() => Ok("controller-query");
```

Requests sent with `curl.exe -X QUERY` returned `200 OK` from both endpoints.
The repository application was not modified during this probe.

## Recommended Implementation

Keep the existing controller architecture. Add a dedicated controller action
using:

```csharp
[AcceptVerbs("QUERY")]
[Route("products/query")]
```

This is the smallest change consistent with the current API. It keeps CRUD
controllers and the query endpoint in the same routing model, while explicitly
proving that the method is `QUERY` rather than a POST substitute.

The action should bind a structured JSON request body, validate it, and pass a
`CancellationToken` to the Cosmos query operation. The query body should not
contain arbitrary Cosmos SQL.

## Exact API/Routing Mechanism

Recommended controller shape:

```csharp
[ApiController]
[Route("products")]
public sealed class ProductsController : ControllerBase
{
    [AcceptVerbs("QUERY")]
    [Route("query")]
    public async Task<IActionResult> Query(
        ProductQueryRequest request,
        CancellationToken cancellationToken)
    {
        // Task 8 implementation
    }
}
```

The route will be:

```text
QUERY /products/query
```

The request body can be JSON with `Content-Type: application/json`. The method
name is case-sensitive on the wire by HTTP convention; sample requests should
use uppercase `QUERY`.

## Controller or Minimal API Recommendation

Use controllers.

The existing API already uses controller-based attribute routing, and the
local probe confirmed that `[AcceptVerbs("QUERY")]` works directly. Moving the
query endpoint to Minimal APIs would create an unnecessary architectural split.

Minimal APIs remain a valid alternative because `MapMethods` explicitly accepts
arbitrary method strings, but that is not the best fit for this repository.

## Swagger/OpenAPI Support

The repository currently has no Swagger/OpenAPI tooling, so there is no current
Swagger behavior to verify.

Important version distinction:

- ASP.NET Core 10 built-in OpenAPI generation produces OpenAPI 3.1 documents.
- OpenAPI 3.2 adds a standard `query` Path Item operation key.
- ASP.NET Core 11 is documented as the first built-in version that generates
  OpenAPI 3.2 documents.
- ASP.NET Core 10 documentation states that unknown HTTP methods such as QUERY
  are excluded from generated OpenAPI documents.

Therefore, under the installed ASP.NET Core 10 stack, the planned QUERY endpoint
should be considered a runtime endpoint that is tested through `curl.exe` and
`api.http`, not an operation that Swagger/OpenAPI will reliably display.

Do not add a fake `POST /query` operation solely to make a UI display it. If
OpenAPI documentation is added later, document the omission or use a custom
OpenAPI transformation only after verifying the generated document and the
consumer tooling. A custom transformation may produce a non-interoperable
OpenAPI 3.1 document because `query` is not a valid operation key in that
version.

## Alternative Testing Methods

### curl.exe

Windows curl can send the method directly:

```powershell
curl.exe -i -X QUERY http://localhost:5099/products/query `
  -H "Content-Type: application/json" `
  -H "Accept: application/json" `
  --data "{\"category\":\"Electronics\",\"pageSize\":10}"
```

When the API uses HTTPS, use the trusted emulator/API certificate. Do not add
`-k` to normal API verification as a permanent workaround.

### VS Code .http files

The VS Code REST Client syntax supports an explicit method token, so the sample
file can contain:

```http
QUERY http://localhost:5099/products/query
Content-Type: application/json
Accept: application/json

{
  "category": "Electronics",
  "pageSize": 10
}
```

The request must be sent as `QUERY`, not renamed to POST.

### Raw HTTP clients

Any HTTP client that allows a custom method string can send QUERY, including
`HttpClient` with `new HttpMethod("QUERY")`. This is useful for automated routing
checks but should not replace a real end-to-end API test.

## Risks and Limitations

1. QUERY is still a draft method specification; intermediaries and client tools
   may not recognize or forward it consistently.
2. ASP.NET Core 10 runtime routing supports the method, but its built-in OpenAPI
   generator excludes unknown methods.
3. Older proxies, gateways, WAFs, or test tools may return `405` or `501` before
   the request reaches the application.
4. Swagger UI is not currently installed and should not be treated as the
   primary QUERY test client.
5. A QUERY request body is semantically appropriate under the draft, but the
   application must validate its media type and structured schema.
6. The emulator certificate trust issue currently blocks application-level
   Cosmos operations. Routing tests can run without Cosmos connectivity; query
   execution tests require the certificate issue to be resolved.
7. The query endpoint must remain parameterized and must not accept raw Cosmos
   SQL from normal clients.

## Recommended Next Step

Review and approve this report. After approval, implement Task 8 using the
controller mechanism and route `QUERY /products/query`.

Task 8 must begin with a structured request model, then add simple filters,
numeric filters, combined conditions, and allow-listed sorting. The first
verification must send a literal QUERY request with `curl.exe` and record
whether Cosmos execution was blocked by the certificate environment.

## Research References

- [ASP.NET Core controller routing](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-10.0)
- [ASP.NET Core Minimal API route handlers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers?view=aspnetcore-10.0)
- [HttpMethodAttribute](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.routing.httpmethodattribute?view=aspnetcore-10.0)
- [AcceptVerbsAttribute](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.acceptverbsattribute?view=aspnetcore-10.0)
- [ASP.NET Core OpenAPI generation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0)
- [OpenAPI Specification 3.2 Path Item Object](https://spec.openapis.org/oas/latest.html#path-item-object)
- [RFC 9110 HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110.html)
- [IETF HTTP QUERY draft](https://www.ietf.org/archive/id/draft-ietf-httpbis-safe-method-w-body-02.html)
