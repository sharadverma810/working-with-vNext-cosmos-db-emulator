TASK 1 — Analyze the Existing Project
You are working on an existing .NET project.

Do not modify any files yet.

Your task is to analyze the existing solution before implementation.

Inspect:

1. Solution structure
2. All projects
3. .NET versions
4. NuGet packages
5. Existing API architecture
6. Existing Cosmos DB integration
7. Existing configuration
8. Existing domain models
9. Existing CRUD functionality
10. Existing tests
11. Existing scripts
12. README.md and documentation

Determine:

- How the application is structured.
- Whether Cosmos DB is already configured.
- Which database and container configuration exists.
- Whether a partition key is already configured.
- Which Cosmos SDK version is being used.
- Whether the application uses Controllers or Minimal APIs.
- Whether Swagger/OpenAPI is configured.
- Whether dependency injection is already configured.

Then provide the following report:

## Current Solution Structure

## Existing Architecture

## Existing Cosmos DB Integration

## Existing API Endpoints

## Existing Configuration

## Existing Documentation

## Existing Tests

## Components That Can Be Reused

## Missing Functionality

## Potential Risks

## Recommended Implementation Plan

Important:

Do not modify, create, delete, or rename any files.

Only analyze the project and provide the report.

Wait for approval before implementing anything.


TASK 2 — Verify Cosmos DB vNext Connectivity

Only proceed after reviewing Task 1.

Based on the previous project analysis, implement only the Cosmos DB connection and connectivity verification improvements.

Do not implement bootstrap, CRUD, QUERY functionality, or data generation yet.

First inspect the existing Cosmos DB configuration.

Reuse existing configuration and architecture wherever possible.

The application should connect to the local Azure Cosmos DB Emulator vNext.

Expected local endpoint:

https://localhost:8081

Requirements:

1. Do not hard-code configuration unnecessarily.
2. Use the existing configuration approach.
3. Use Dependency Injection.
4. Reuse an existing CosmosClient if one exists.
5. Do not create multiple CosmosClient instances unnecessarily.
6. Add clear startup validation where appropriate.
7. Provide useful errors when Cosmos DB cannot be reached.
8. Do not log account keys or secrets.
9. Do not modify unrelated code.

After implementation:

1. Run dotnet restore.
2. Run dotnet build.
3. Fix compilation errors.
4. Report what was changed.

Do not claim that Cosmos DB connectivity was verified unless you actually verified it.

Provide:

- Files modified
- Configuration required
- How to verify connectivity
- Build result
- Any limitations


TASK 3 — Create the Rich Sample Document Model

This task creates the foundation for your 5,000 documents.

Based on the existing project architecture, implement only the sample document model.

Do not implement data generation yet.

Do not implement bootstrap.

Do not implement API endpoints.

Analyze the existing domain models first.

If a suitable model already exists, extend it instead of replacing it.

Otherwise, create a realistic Product Catalog document model suitable for learning Azure Cosmos DB queries.

The document should support:

1. Strings
2. Integers
3. Decimal/numeric values
4. Boolean values
5. Nullable values
6. Date/time values
7. Arrays
8. Nested objects
9. Nested arrays

The primary document should include meaningful fields such as:

Basic information:

- id
- productName
- description
- category
- subCategory
- status

Numeric information:

- price
- discount
- rating
- stockQuantity

Boolean fields:

- isActive
- isFeatured
- isAvailable

Dates:

- createdAt
- updatedAt
- availableFrom

Arrays:

- tags
- supportedCountries
- relatedProducts

Nested objects:

manufacturer

warehouseLocation

metadata

audit

Nested arrays:

reviews

Example concepts:

manufacturer:
- name
- country
- website

warehouseLocation:
- country
- state
- city
- postalCode

review:
- reviewerName
- rating
- comment
- reviewDate

metadata:
- source
- version
- importedAt

audit:
- createdBy
- updatedBy

Requirements:

1. Use clear naming.
2. Follow the existing project's naming conventions.
3. Use appropriate .NET types.
4. Ensure Cosmos DB serialization works correctly.
5. Do not add unnecessary abstractions.
6. Do not create repositories unnecessarily.

Also analyze the partition key.

If one already exists, do not change it without explaining why.

If one does not exist, recommend a partition key suitable for this learning project.

Do not create the container yet.

After implementation:

- Build the project.
- Report files created.
- Report files modified.
- Explain the document structure.
- Explain the recommended partition key.


TASK 4 — Bootstrap 5,000 Documents

This is one of the most important tasks.

Implement only the bootstrap/data initialization process.

Do not implement CRUD or QUERY APIs yet.

First analyze the existing solution structure.

Choose the simplest bootstrap approach that fits the existing project.

Prefer a separate .NET console/bootstrap project if it integrates cleanly with the solution.

The bootstrap process should:

1. Connect to Azure Cosmos DB Emulator vNext.
2. Create the database if it does not exist.
3. Create the container if it does not exist.
4. Use the selected partition key.
5. Generate approximately 5,000 sample documents.
6. Insert the documents.
7. Display useful progress information.
8. Display a completion summary.

The generated documents must contain meaningful variety.

Do not generate 5,000 nearly identical documents.

Generate variations in:

- category
- subCategory
- status
- price
- discount
- rating
- stockQuantity
- isActive
- isFeatured
- isAvailable
- dates
- tags
- manufacturers
- countries
- cities
- reviews
- nullable properties

Use deterministic data generation where practical.

Prefer a fixed random seed so the dataset is reproducible.

Data generation should support future query demonstrations including:

- equality
- numeric ranges
- dates
- booleans
- nulls
- nested objects
- arrays
- nested arrays
- sorting

Insertion requirements:

1. Investigate the Cosmos SDK features available.
2. Use a reliable approach.
3. Do not create excessive parallel requests.
4. If concurrency is used, limit it.
5. Make progress visible.

Example progress:

Generating 5,000 documents...

Inserted 500 / 5,000

Inserted 1,000 / 5,000

...

Bootstrap completed successfully.

Completion summary should include:

- Database name
- Container name
- Number of documents generated
- Number successfully inserted
- Failed inserts
- Execution duration

Important:

Do not silently ignore failures.

After implementation:

1. Run dotnet restore.
2. Run dotnet build.
3. Fix compilation errors.
4. If the emulator is available, run the bootstrap.
5. Report actual results.

Do not claim 5,000 documents were inserted unless the process was actually executed successfully.

TASK 5 — Reset the Sample Data

Implement only the reset process.

First inspect the bootstrap implementation.

Reuse as much of the bootstrap logic as possible.

Do not duplicate large amounts of code.

The reset process should:

1. Only affect the database/container used by this sample project.
2. Remove the existing sample data.
3. Recreate the dataset.
4. Generate approximately 5,000 documents.
5. Work repeatedly.

The reset process should be idempotent.

Possible approaches:

Option A:
Delete the sample container and recreate it.

Option B:
Delete all sample documents and regenerate them.

Analyze which option is simpler and safer.

Prefer the approach that:

- Is easy to understand.
- Is reliable.
- Does not affect unrelated Cosmos DB resources.

The reset command should be easy to run.

For example:

dotnet run --project <project>

with a reset option.

Or a dedicated reset project/script.

Reuse existing architecture.

Do not implement CRUD or QUERY functionality.

After implementation:

1. Build the solution.
2. If possible, execute the reset.
3. Verify approximately 5,000 documents exist.
4. Report actual results.

Provide:

- Reset command
- What resources are affected
- What resources are not affected
- Files modified
- Files created

TASK 6 — Implement CRUD API

Implement CRUD functionality only.

Do not implement the HTTP QUERY method yet.

First inspect existing API endpoints.

Reuse existing architecture.

Do not replace working endpoints unnecessarily.

Implement or improve:

1. CREATE
2. POINT READ
3. LIST
4. UPDATE
5. DELETE

CREATE:

POST

Create a sample document.

Return:

201 Created

when successful.

READ:

GET

Retrieve a document using:

- id
- partition key

Use Cosmos DB ReadItemAsync or the appropriate point read operation.

This is important.

The project should clearly demonstrate the difference between:

POINT READ

and:

QUERY

LIST:

Retrieve multiple documents.

Do not overcomplicate this endpoint.

UPDATE:

Use the existing project's approach.

PUT may be used for complete replacement.

If Cosmos DB Patch functionality is already appropriate for the project, it may be added separately later.

DELETE:

Delete using:

- id
- partition key

Requirements:

1. Async operations.
2. CancellationToken where appropriate.
3. Proper validation.
4. Appropriate HTTP status codes.
5. Consistent error handling.
6. Do not expose stack traces.
7. Reuse existing logging.

Handle appropriate Cosmos errors:

400
404
409
429
500

Do not add unnecessary Repository patterns.

Do not add unnecessary interfaces.

After implementation:

1. Build.
2. Test endpoints if possible.
3. Report what was actually tested.

Provide example requests.


TASK 7 — Research HTTP QUERY Before Coding

This is the most important control point.

Do NOT implement the HTTP QUERY endpoint yet.

Perform research and technical verification only.

We are using the actual HTTP QUERY method as a learning objective.

Investigate the installed versions of:

- .NET
- ASP.NET Core
- Web framework
- OpenAPI/Swagger tooling

Determine:

1. Does the installed ASP.NET Core version support custom HTTP methods?
2. Can an endpoint be registered for the HTTP method:

QUERY

3. What is the correct implementation approach?
4. Can Controllers support this method?
5. Can Minimal APIs support this method?
6. Which approach fits the existing project architecture?
7. Does Swagger/OpenAPI support QUERY?
8. If Swagger does not support QUERY, how should it be tested?
9. Can .http files send a QUERY request?
10. Can curl.exe send a QUERY request?

Do not guess.

Use actual framework documentation and the installed project dependencies where necessary.

Do not implement:

POST /query

as a substitute.

We specifically want to understand the actual:

QUERY

HTTP method.

Provide a technical report containing:

## Installed Versions

## HTTP QUERY Support

## Recommended Implementation

## Exact API/Routing Mechanism

## Controller or Minimal API Recommendation

## Swagger/OpenAPI Support

## Alternative Testing Methods

## Risks and Limitations

## Recommended Next Step

Do not modify application code.

Wait for approval.


TASK 8 — Implement the Actual HTTP QUERY Method

Only after Task 7 is reviewed.

Based on the verified findings from the previous HTTP QUERY research, implement the actual HTTP method:

QUERY

Do not replace it with POST.

Use the correct implementation mechanism for the installed ASP.NET Core version and existing architecture.

First implement a structured query request.

Do not accept arbitrary raw Cosmos DB SQL from normal API clients.

The query request should support optional filters such as:

- category
- subCategory
- status
- minimumPrice
- maximumPrice
- minimumRating
- isActive
- isFeatured
- createdAfter
- createdBefore
- city
- tags

Also support:

- sortField
- sortDirection
- pageSize
- continuationToken

Validate:

pageSize

with a reasonable maximum.

Use parameterized Cosmos DB queries.

Use:

QueryDefinition

and parameters.

Do not concatenate user values into Cosmos SQL.

For example:

Correct:

WithParameter("@category", value)

Incorrect:

"... category = '" + value + "'"

Sorting:

Do not directly insert arbitrary user values into ORDER BY.

Use an allow-list.

For example:

Allowed fields:

- price
- rating
- createdAt
- productName

Validate sort direction.

Allowed:

ASC

DESC

Implement the endpoint incrementally.

Start with:

1. Simple filters.
2. Numeric filters.
3. Multiple conditions.
4. Sorting.

Do not implement all advanced query scenarios yet.

Return:

- items
- count
- continuationToken

Where available and useful, also include:

- requestCharge
- activityId

Do not expose sensitive information.

After implementation:

1. Build.
2. Verify routing.
3. Test the actual HTTP QUERY request.
4. Do not claim success unless QUERY was actually sent.

Provide an exact test command.

Prefer:

curl.exe

for testing.

Also update:

scripts/sample-requests/api.http

with an actual QUERY request.

Report:

- Actual endpoint
- How it was implemented
- How to test it
- Whether it was actually tested
- Any Swagger limitations


TASK 9 — Add Advanced Query Scenarios
Extend the existing HTTP QUERY implementation.

Do not rewrite the existing query implementation.

Do not replace working code.

Add advanced Cosmos DB query scenarios incrementally.

Support and demonstrate:

## 1. Date Queries

Examples:

createdAfter

createdBefore

Date range.

---

## 2. Null Queries

Demonstrate:

- Null values.
- Missing properties.

Document Cosmos DB behavior.

Do not assume null and undefined are identical.

---

## 3. Nested Object Queries

Examples:

manufacturer.country

warehouseLocation.city

metadata.source

---

## 4. Array Queries

Examples:

tags

supportedCountries

Use appropriate Cosmos DB query functions.

---

## 5. Nested Array Queries

Use:

reviews

or another nested array.

Demonstrate appropriate Cosmos DB syntax.

---

## 6. Complex Query

Create a realistic combination.

For example:

Category

AND

Active

AND

Price range

AND

Minimum rating

AND

Specific tag.

---

Requirements:

1. Continue using parameterized queries.
2. Maintain query safety.
3. Keep query construction understandable.
4. Do not over-engineer.
5. Reuse existing query logic.
6. Avoid duplicated query-building code.

Update:

scripts/sample-requests/api.http

Add a request for each scenario.

Add comments explaining:

- What the request demonstrates.
- What Cosmos DB feature it uses.

Build and test where possible.

Report:

- New scenarios.
- Files modified.
- What was tested.
- What could not be tested.

TASK 10 — Pagination and Cosmos Learning Features

Improve the existing API to demonstrate important Cosmos DB concepts.

Do not add features unnecessarily.

Focus on learning value.

Implement or demonstrate:

## 1. Continuation Tokens

The QUERY response should include:

continuationToken

The next request should be able to send it back.

Do not use OFFSET/LIMIT as the primary pagination approach.

---

## 2. Request Charge

Where available:

Expose the Cosmos DB request charge.

---

## 3. Activity ID

Where useful:

Expose Activity ID.

---

## 4. ETags

Demonstrate ETags.

Explain how ETags can be used.

---

## 5. Optimistic Concurrency

If practical:

Demonstrate optimistic concurrency.

Do not make the CRUD API unnecessarily complex.

---

## 6. Patch Operations

Evaluate whether Cosmos DB PATCH operations would provide useful learning value.

If implemented:

Create a clear PATCH endpoint.

Do not replace PUT.

Demonstrate partial updates.

---

## 7. Cosmos Exceptions

Ensure common Cosmos errors are handled appropriately.

Examples:

404

409

429

---

Update sample requests.

Add examples demonstrating:

- Pagination.
- Continuation token usage.
- ETag behavior if implemented.
- Patch operations if implemented.

Build and test.

Report actual verification.


TASK 11 — Tests and Code Review

I recommend adding this as a separate task.

Perform a code review of the project.

Do not rewrite the project.

Analyze:

1. Architecture.
2. Code duplication.
3. Error handling.
4. Configuration.
5. CosmosClient lifecycle.
6. Async usage.
7. CancellationToken usage.
8. Query safety.
9. Sorting safety.
10. Logging.
11. Exception handling.
12. Security concerns.

Identify improvements.

Then implement only meaningful improvements.

Do not:

- Introduce unnecessary patterns.
- Introduce excessive interfaces.
- Add CQRS unnecessarily.
- Add MediatR unnecessarily.
- Add repositories unnecessarily.

Add useful automated tests.

Focus on:

1. Query request validation.
2. Sort field allow-list.
3. Sort direction validation.
4. Query construction logic.
5. Important edge cases.

Do not make the project difficult to run.

Run:

dotnet build

Run tests.

Report:

- Tests added.
- Build result.
- Test result.
- Improvements made.


TASK 12 — README and Final Verification

The final task.

Perform final project verification and documentation.

Do not claim anything was verified unless it was actually executed.

First inspect the entire current solution.

Then update README.md.

The README should allow a new developer to start from scratch.

Include:

# Project Overview

Explain what the project demonstrates.

---

# Architecture

Include:

Client
   ↓
ASP.NET Core API
   ↓
Microsoft.Azure.Cosmos SDK
   ↓
Azure Cosmos DB Emulator vNext
   ↓
Docker

---

# Prerequisites

Document:

- Windows
- Docker Desktop
- WSL 2
- .NET 10 SDK
- Git

---

# Start Cosmos DB Emulator

Provide exact commands.

Include:

docker ps

docker logs cosmos-vnext --tail 50

Explain expected healthy output.

---

# Certificate Setup

Provide separate instructions.

## Windows PowerShell 5.1

Use:

curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt

Then:

Import-Certificate `
  -FilePath .\emulatorcert.crt `
  -CertStoreLocation Cert:\CurrentUser\Root

Explain:

PowerShell 5.1 does not support:

SkipCertificateCheck

with Invoke-WebRequest.

---

## PowerShell 7+

Provide the PowerShell 7+ approach.

Keep:

SkipCertificateCheck = $True

---

# Clone Project

Provide exact commands.

---

# Restore and Build

Provide:

dotnet restore

dotnet build

---

# Configuration

Explain:

- Cosmos endpoint.
- Database.
- Container.
- Account key.
- Environment variables.

Do not expose secrets.

---

# Bootstrap Data

Provide exact command.

Explain:

1. Database creation.
2. Container creation.
3. Approximately 5,000 documents.
4. Progress output.

---

# Verify Data

Provide a way to verify data.

For example:

API endpoint

or:

Cosmos Data Explorer.

---

# Reset Data

Provide exact command.

Clearly explain:

What is deleted.

What is recreated.

What is NOT affected.

---

# Run API

Provide:

dotnet run

Explain:

API URL.

Swagger URL.

---

# CRUD Examples

Provide:

Create.

Point Read.

List.

Update.

Delete.

---

# HTTP QUERY Method

This section is important.

Clearly explain:

The project uses the actual:

QUERY

HTTP method.

Document:

- Endpoint.
- Request body.
- Query filters.
- Pagination.
- Continuation tokens.

Explain how to test QUERY.

If Swagger does not support QUERY correctly:

Explain the limitation.

Provide:

curl.exe

and:

api.http

examples.

---

# Query Examples

Progressively demonstrate:

1. Simple category filter.
2. Price range.
3. Multiple filters.
4. Sorting.
5. Date range.
6. Nested object.
7. Array.
8. Nested array.
9. Complex query.
10. Pagination.

Explain each.

---

# Project Structure

Explain important projects and folders.

---

# Troubleshooting

Include:

## Docker Desktop is not running.

## Emulator container is not running.

## Port 8081 already in use.

## Cosmos container name already exists.

Example:

docker rm -f cosmos-vnext

## Certificate error.

## Windows PowerShell 5.1 error.

## Cosmos connection failure.

## Bootstrap failure.

## API cannot connect.

## QUERY endpoint cannot be tested through Swagger.

---

# Final Verification

Perform the following where possible:

1. dotnet restore
2. dotnet build
3. Run tests
4. Bootstrap data
5. Verify approximately 5,000 documents
6. Run reset
7. Run API
8. Test CRUD
9. Test actual QUERY
10. Test pagination

Clearly state:

✓ Successfully verified

and:

⚠ Not verified

Do not falsely claim success.

---

# Final Report

Provide:

## Project Summary

## Files Created

## Files Modified

## Bootstrap Command

## Reset Command

## Run API Command

## CRUD Test Example

## HTTP QUERY Test Example

## Build Result

## Test Result

## Known Limitations

## Recommended Next Learning Steps


