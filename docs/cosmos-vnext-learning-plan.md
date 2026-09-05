# Cosmos DB Emulator vNext Learning Project

## Purpose

Create a complete local .NET 10 learning project for Azure Cosmos DB Emulator vNext on Windows using Docker Desktop and WSL 2.

The project will demonstrate:

- Cosmos DB API for NoSQL
- Microsoft.Azure.Cosmos SDK
- Configuration and dependency injection
- Product Catalog documents
- Partition keys
- Bootstrap and reset workflows
- CRUD operations
- Point reads versus queries
- Parameterized Cosmos DB SQL
- The actual HTTP QUERY method
- Simple, advanced, nested, array, and null queries
- Continuation-token pagination
- Request charge, activity IDs, ETags, optimistic concurrency, and optional PATCH operations
- Automated tests and a complete developer README

This repository began as documentation only. There is no existing .NET solution, API, project, test suite, or implementation to preserve. The implementation must therefore create the solution incrementally while preserving the existing emulator documentation.

## Governing Rules

1. Execute tasks in order. Do not implement a later task before its prerequisites and approval gate are complete.
2. Inspect the current state before every implementation task.
3. Make the smallest change that satisfies the task.
4. Do not add generic repositories, CQRS, MediatR, microservices, event sourcing, or unnecessary abstractions.
5. Use one dependency-injected `CosmosClient`; do not create clients per request.
6. Do not expose account keys, secrets, stack traces, or arbitrary Cosmos SQL.
7. Use asynchronous APIs and cancellation tokens where appropriate.
8. Do not claim connectivity, insertion counts, endpoint behavior, or test success unless it was actually executed.
9. Keep the application local-only. Azure subscriptions, private endpoints, VNets, MongoDB API workloads, and production deployment are outside this project.
10. The actual HTTP `QUERY` method is required. `POST /query` is not an acceptable substitute.
11. Partition-key alternatives must be evaluated before the database container is created. `/category` is a candidate, not a predetermined decision.
12. Update the README only after the commands and behaviors documented there have been verified.

## Target Repository Structure

- `src/CosmosLearning.Api/` - ASP.NET Core Web API
- `src/CosmosLearning.Bootstrap/` - bootstrap and reset console project
- `tests/CosmosLearning.Tests/` - focused automated tests
- `scripts/sample-requests/api.http` - executable sample requests
- `docs/cosmos-vnext-learning-plan.md` - this canonical plan
- `README.md` - final newcomer-facing runbook
- `Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md` - emulator environment reference

## Task 1 - Analyze the Existing Project

### Objective

Establish an accurate baseline before creating application code.

### Actions

1. Inspect the full repository, including hidden folders.
2. Identify solution files, projects, source code, configuration, tests, scripts, and documentation.
3. Inspect .NET versions, package versions, API architecture, dependency injection, Cosmos integration, domain models, CRUD functionality, and existing tests.
4. Reconcile the analysis with the fact that the current repository is documentation-only.
5. Review `README.md`, the emulator guide, and all existing prompt files.
6. Record components that can be reused and functionality that is missing.

### Deliverable

A baseline report containing:

- Current solution structure
- Existing architecture
- Existing Cosmos DB integration
- Existing API endpoints
- Existing configuration
- Existing documentation
- Existing tests
- Reusable components
- Missing functionality
- Risks
- Recommended implementation sequence

### Gate

Do not claim that an existing .NET architecture or Cosmos integration exists. Proceed only after this analysis is reviewed.

## Task 2 - Verify Cosmos DB vNext Connectivity

### Objective

Create the initial solution and prove that the application can connect to the local emulator.

### Actions

1. Verify Docker Desktop, WSL 2, .NET 10 SDK, emulator image, ports `8081`, `8080`, and `1234`, and the emulator readiness endpoint.
2. Create the approved API, bootstrap, and test project structure.
3. Use .NET 10, ASP.NET Core Web API, C#, and `Microsoft.Azure.Cosmos`.
4. Check current compatible package versions before adding or changing packages.
5. Add configuration for endpoint, account key, database, and container using configuration providers and environment-variable overrides.
6. Do not hard-code secrets.
7. Register one reusable `CosmosClient` through dependency injection.
8. Add startup/configuration validation and useful connection failures without logging keys.
9. Keep the initial implementation limited to connectivity. Do not add bootstrap, CRUD, query, or data generation.

### Verification

Run:

- `dotnet restore`
- `dotnet build`
- `curl.exe http://localhost:8080/ready`
- an application-level Cosmos connectivity operation when the emulator is available

### Deliverable

Report files modified/created, required configuration, exact connectivity verification steps, build result, and limitations. Clearly distinguish reachable HTTP ports from verified SDK connectivity.

## Task 3 - Create the Rich Sample Document Model

### Objective

Create the Product Catalog document foundation without creating the container or implementing APIs.

### Actions

1. Inspect any existing domain model first; because the baseline is empty, create a focused model in the API/shared location selected during Task 2.
2. Include `id`, `productName`, `description`, `category`, `subCategory`, `status`, `price`, `discount`, `rating`, `stockQuantity`, `isActive`, `isFeatured`, `isAvailable`, `createdAt`, `updatedAt`, `availableFrom`, nullable properties, `tags`, `supportedCountries`, `relatedProducts`, `manufacturer`, `warehouseLocation`, `metadata`, `audit`, and nested `reviews`.
3. Use clear naming, appropriate .NET types, UTC date/time handling, and Cosmos-compatible serialization.
4. Avoid unnecessary abstractions and repositories.
5. Evaluate partition-key candidates against point reads, query demonstrations, partition distribution, and cross-partition learning value.
6. Document the selected partition key decision before container creation.

### Verification

Build the project and test serialization, required fields, nullable fields, nested objects, arrays, and nested arrays.

### Deliverable

Report model files, document structure, serialization behavior, partition-key alternatives, final recommendation, and rationale.

## Task 4 - Bootstrap 5,000 Documents

### Objective

Create the database/container and populate approximately 5,000 varied, deterministic documents.

### Actions

1. Inspect the solution before choosing the bootstrap mechanism.
2. Prefer the separate bootstrap console project if it integrates cleanly.
3. Reuse configuration, the single-client approach, the document model, and the generator.
4. Create the database if absent.
5. Create the container if absent using the selected partition key.
6. Generate approximately 5,000 documents with a fixed seed and meaningful variation in categories, subcategories, statuses, numeric values, booleans, dates, tags, manufacturers, countries, cities, reviews, arrays, and nullable values.
7. Ensure the dataset supports equality, ranges, dates, booleans, nulls, nested fields, arrays, nested arrays, and sorting.
8. Investigate supported Cosmos SDK insertion options.
9. Use a reliable sequential or bounded-concurrency strategy. Do not overwhelm the emulator.
10. Display progress such as inserted count and total.
11. Do not silently ignore failures. Support cancellation and report partial failures.
12. Print database name, container name, generated count, successful inserts, failed inserts, and duration.

### Verification

Run restore and build. If the emulator is available, execute the bootstrap and verify the actual document count and data variety. Do not claim 5,000 inserted documents unless verified.

### Deliverable

Report commands, files, insertion strategy, actual run result, failures, and limitations.

## Task 5 - Reset the Sample Data

### Objective

Provide a safe, repeatable reset that affects only this sample dataset.

### Actions

1. Inspect and reuse the bootstrap implementation.
2. Avoid duplicating generator or Cosmos setup logic.
3. Compare deleting/recreating the sample container with deleting generated documents.
4. Choose the simplest reliable approach that cannot affect unrelated resources.
5. Make the reset command explicit and easy to run.
6. Ensure repeated execution is idempotent.
7. Recreate approximately 5,000 deterministic documents.
8. Document exactly what is deleted, recreated, and left untouched.

### Verification

Build the solution, run reset when possible, verify the resulting count, and run reset again to prove repeatability.

### Deliverable

Report reset command, affected resources, unaffected resources, files, and actual verification results.

## Task 6 - Implement CRUD API

### Objective

Implement create, point read, list, update, and delete without implementing the HTTP QUERY endpoint.

### Actions

1. Inspect the current API surface before adding endpoints.
2. Reuse the established configuration, client, model, logging, and error-handling approach.
3. Implement `POST` create with `201 Created`, point `GET` using both id and partition key with Cosmos point-read APIs, list `GET`, `PUT` complete replacement, and `DELETE` using id and partition key.
4. Use async operations and cancellation tokens.
5. Validate input and return appropriate status codes.
6. Handle 400, 404, 409, 429, and 500 safely without stack traces.
7. Keep point reads visibly distinct from queries.
8. Evaluate ETags and PATCH later; do not complicate core CRUD prematurely.

### Verification

Build and exercise create, point read, list, update, and delete against the emulator where available. Report only operations actually tested and include example requests.

## Task 7 - Research HTTP QUERY Before Coding

### Objective

Technically verify support for the literal HTTP `QUERY` method before changing application code.

### Actions

1. Inspect installed .NET, ASP.NET Core, web framework, and Swagger/OpenAPI versions.
2. Use current authoritative documentation and local verification where required.
3. Determine custom-method support, endpoint registration, controller support, Minimal API support, the best architectural fit, Swagger/OpenAPI limitations, `.http` support, `curl.exe` support, request-body behavior, and routing behavior.
4. Do not implement `POST /query` as a substitute.
5. Do not modify application code during this research task.

### Deliverable

A technical report containing Installed Versions, HTTP QUERY Support, Recommended Implementation, Exact API/Routing Mechanism, Controller or Minimal API Recommendation, Swagger/OpenAPI Support, Alternative Testing Methods, Risks and Limitations, and Recommended Next Step.

### Gate

Wait for review and approval before implementing the actual QUERY endpoint.

## Task 8 - Implement the Actual HTTP QUERY Method

### Objective

Implement the literal HTTP `QUERY` method using the verified mechanism.

### Actions

1. Implement the actual `QUERY` route. Do not replace it with POST.
2. Add a structured request model supporting category, subCategory, status, minimum/maximum price, minimum rating, isActive, isFeatured, createdAfter/createdBefore, city, tags, sortField, sortDirection, pageSize, and continuationToken.
3. Validate page size using a documented maximum.
4. Use `QueryDefinition` and parameters for all user values.
5. Never concatenate user values into Cosmos SQL.
6. Allow-list sortable fields such as price, rating, createdAt, and productName.
7. Validate sort direction as ASC or DESC.
8. Implement incrementally: simple filters, numeric filters, combined conditions, and sorting.
9. Return items, count, and continuation token; include request charge and activity ID where safely available.
10. Add the literal QUERY request to `scripts/sample-requests/api.http`.

### Verification

Build, verify routing, and send an actual `QUERY` request with `curl.exe`. Do not claim success unless the literal method was sent and processed. Document Swagger limitations.

## Task 9 - Add Advanced Query Scenarios

### Objective

Extend the existing QUERY implementation without rewriting it.

### Actions

Add parameterized, understandable scenarios for date ranges; explicit null and missing-property behavior; nested objects such as `manufacturer.country`, `warehouseLocation.city`, and `metadata.source`; arrays such as `tags` and `supportedCountries`; nested arrays such as `reviews`; and a complex combination of category, active status, price range, rating, and tag.

Update `api.http` with one explanatory request per scenario.

### Verification

Build and test scenarios where the emulator supports them. Report verified and unverified cases separately.

## Task 10 - Pagination and Cosmos Learning Features

### Objective

Add high-value Cosmos learning features without unnecessary complexity.

### Actions

1. Use continuation tokens as the primary pagination mechanism, not OFFSET/LIMIT.
2. Return continuation tokens and accept them in subsequent QUERY requests.
3. Expose request charge where available.
4. Expose activity ID where useful and safe.
5. Evaluate ETags and demonstrate conditional updates if practical.
6. Evaluate optimistic concurrency without making CRUD difficult to understand.
7. Evaluate a separate PATCH endpoint without replacing PUT.
8. Preserve common Cosmos exception handling, especially 404, 409, and 429.
9. Update sample requests for pagination, continuation tokens, ETags, and PATCH only when implemented.

### Verification

Build and test continuation-token round trips, metadata, and any implemented ETag/PATCH behavior. Clearly report unsupported or unverified emulator features.

## Task 11 - Tests and Code Review

### Objective

Review the finished implementation and add only meaningful automated coverage.

### Review Areas

- Architecture and unnecessary duplication
- Configuration and secret handling
- CosmosClient lifecycle
- Async and cancellation usage
- Query safety and parameterization
- Sort allow-list and direction validation
- Logging and exception handling
- Cosmos error mapping
- Security concerns

### Tests

Add focused tests for query request validation, page-size limits, sort-field allow-list, sort-direction validation, query construction and parameter binding, generator determinism, and important edge cases. Add emulator integration tests only if they are reliable, documented, and runnable locally.

### Verification

Run `dotnet build` and `dotnet test`. Implement only meaningful improvements; do not introduce unnecessary architecture.

## Task 12 - README and Final Verification

### Objective

Make the repository usable by a developer starting from scratch and report only actual verification.

### README Sections

Rewrite `README.md` to include project overview; architecture from client through Docker; prerequisites; exact emulator startup, status, logs, and readiness commands; expected healthy output; Windows PowerShell 5.1 certificate setup using `curl.exe -k`; PowerShell 7+ certificate setup using `SkipCertificateCheck`; clone, restore, and build; configuration; bootstrap; data verification; reset scope; API and Swagger; CRUD; actual HTTP QUERY; curl and `api.http` testing; progressive query examples; project structure; troubleshooting; final verification; final report; and next learning steps.

### Final Verification

Run where possible:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
4. Emulator bootstrap
5. Document-count and variety verification
6. Reset and repeat reset
7. API startup
8. CRUD tests
9. Actual HTTP QUERY test
10. Pagination and continuation-token test

Mark each item as Successfully verified, Not verified, or Blocked. Never claim successful insertion, connectivity, routing, or testing without execution evidence.

### Required Final Report

Include project summary, reused components, files created, files modified, bootstrap command, reset command, API run command, CRUD example, actual HTTP QUERY example, build result, test result, known limitations, and recommended next learning steps.

## Canonical Rule

This file is the single source of truth for implementation. Do not maintain a second competing roadmap. Existing prompt files remain historical requirements input; future implementation follows this plan.
