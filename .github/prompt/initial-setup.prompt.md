# ROLE

You are working as a senior .NET architect and developer.

Your task is to enhance an existing .NET project incrementally.

Do NOT blindly regenerate the solution.

Do NOT delete or replace working functionality.

First analyze the existing project carefully, understand its architecture, and then make the minimum necessary changes to implement the requested functionality.

The project is intended for learning Azure Cosmos DB Emulator vNext locally.

---

# IMPORTANT WORKING APPROACH

Follow this process strictly.

PHASE 1
Analyze the existing project.

PHASE 2
Report your understanding and implementation plan.

PHASE 3
Implement changes incrementally.

PHASE 4
Build and verify.

PHASE 5
Update documentation.

Do not skip directly to implementation without first understanding the existing project.

---

# PROJECT OBJECTIVE

The project should demonstrate the Azure Cosmos DB Emulator vNext running locally.

The main learning goals are:

1. Azure Cosmos DB Emulator vNext
2. Azure Cosmos DB API for NoSQL
3. CRUD operations
4. Point reads
5. Cosmos DB SQL queries
6. The new HTTP QUERY method
7. Partition keys
8. Simple to complex queries
9. Arrays and nested objects
10. Pagination
11. Continuation tokens
12. Resetting local sample data

The project must work completely locally.

Do not require:

- Azure Subscription
- Real Azure Cosmos DB
- Azure resources

The application should connect to:

Azure Cosmos DB Emulator vNext

running locally through Docker.

---

# TECHNOLOGY REQUIREMENTS

Use the existing technology choices where possible.

Do not upgrade packages or frameworks unnecessarily.

The expected technology is:

- .NET 10
- ASP.NET Core Web API
- C#
- Microsoft.Azure.Cosmos SDK
- Azure Cosmos DB Emulator vNext
- Docker Desktop

Before changing package versions:

1. Inspect the currently installed versions.
2. Determine whether they are sufficient.
3. Only change versions when necessary.
4. Explain why the change is necessary.

---

# PHASE 1 — ANALYZE THE EXISTING PROJECT

Before writing code, inspect the entire solution.

Analyze:

## Solution Structure

Identify:

- Solution file
- Projects
- API project
- Domain models
- Application layer
- Infrastructure layer
- Existing tests
- Scripts
- Documentation

---

## Existing Cosmos DB Integration

Determine:

- Whether Microsoft.Azure.Cosmos is already installed
- CosmosClient configuration
- Cosmos endpoint configuration
- Authentication configuration
- Database name
- Container name
- Partition key
- Dependency Injection setup

Do not assume these things.

Inspect the actual implementation.

---

## Existing API

Identify:

- Controllers
- Minimal APIs
- Endpoints
- Request models
- Response models
- Existing CRUD operations
- Existing exception handling
- Validation
- Logging

---

## Existing Configuration

Inspect:

- appsettings.json
- appsettings.Development.json
- Environment variables
- User Secrets if applicable

Do not expose secrets in source code.

---

## Existing Documentation

Inspect:

- README.md
- docs folder
- Docker scripts
- PowerShell scripts
- Bootstrap scripts

---

# PHASE 1 OUTPUT

Before implementing changes, provide a concise analysis containing:

1. Current project structure
2. Current architecture
3. Existing Cosmos DB functionality
4. Existing CRUD functionality
5. Existing configuration approach
6. Existing documentation
7. Missing functionality
8. Proposed implementation plan

Do not modify files during this analysis phase unless required to inspect them.

---

# PHASE 2 — IMPLEMENTATION PLAN

After analyzing the project, create a clear implementation plan.

The plan should contain:

## Existing Components

List components that can be reused.

For example:

- Existing CosmosClient
- Existing configuration
- Existing models
- Existing API controllers
- Existing exception handling

---

## New Components

List components that need to be created.

For example:

- Sample document model
- Data generator
- Bootstrap process
- Reset process
- Query API
- Query request models
- Query service
- Sample HTTP requests

---

## Files To Modify

List the files that will be modified.

---

## Files To Create

List the files that will be created.

---

## Risks

Identify potential technical risks.

Especially investigate:

1. HTTP QUERY method support in .NET 10
2. ASP.NET Core routing support
3. Swagger/OpenAPI support for QUERY
4. Cosmos DB Emulator vNext compatibility
5. Certificate requirements
6. Cosmos DB SDK compatibility

Do not assume.

Verify actual framework capabilities before implementation.

---

# PHASE 3 — IMPLEMENTATION

After completing the analysis and plan, implement the changes incrementally.

Do not rewrite unrelated code.

Do not introduce unnecessary architecture.

Prioritize:

- Simplicity
- Readability
- Learning value
- Working code

---

# SAMPLE DOMAIN

Use a realistic Product Catalog domain unless the existing project already has a suitable domain.

If the project already has a domain model, prefer extending it rather than replacing it.

The domain should support interesting Cosmos DB query scenarios.

A Product Catalog is recommended.

---

# SAMPLE DOCUMENT REQUIREMENTS

Create a rich Cosmos DB document.

The document should contain different data types.

Include:

## Basic Fields

- id
- productName
- description
- category
- subCategory
- status

---

## Numeric Fields

- price
- discount
- rating
- stockQuantity

Use different numeric ranges.

---

## Boolean Fields

Examples:

- isActive
- isFeatured
- isAvailable

---

## Date Fields

Examples:

- createdAt
- updatedAt
- availableFrom

---

## Nullable Fields

Include some properties that can contain:

null

The generated data must contain actual null values for some documents.

---

## Arrays

Include arrays such as:

- tags
- supportedCountries
- relatedProducts

---

## Nested Objects

Include realistic nested objects.

For example:

manufacturer

{
    "name": "",
    "country": "",
    "website": ""
}

---

## Nested Arrays

Include at least one nested array.

For example:

reviews

[
    {
        "reviewerName": "",
        "rating": 5,
        "comment": "",
        "reviewDate": ""
    }
]

---

## Location Object

Include something like:

warehouseLocation

{
    "country": "",
    "state": "",
    "city": "",
    "postalCode": ""
}

---

## Metadata

Include:

metadata

{
    "source": "",
    "version": "",
    "importedAt": ""
}

---

## Audit Information

Include:

audit

{
    "createdBy": "",
    "updatedBy": ""
}

---

# PARTITION KEY

Analyze the existing partition key first.

If no partition key has been selected yet, select an appropriate partition key.

For a Product Catalog:

/category

may be appropriate.

However, do not automatically use it.

Evaluate the query scenarios and document why the partition key was selected.

The README must explain:

1. What the partition key is.
2. Why it was selected.
3. How it affects point reads and queries.

---

# DUMMY DATA

Create approximately:

5,000 documents.

The data must have meaningful variety.

Do not create 5,000 nearly identical documents.

Generate variations in:

- Categories
- Subcategories
- Statuses
- Prices
- Ratings
- Dates
- Boolean values
- Tags
- Manufacturers
- Locations
- Arrays
- Reviews
- Null values

Use deterministic generation where possible.

For example:

a fixed random seed.

This ensures that the dataset can be reproduced.

---

# BOOTSTRAP PROCESS

Create a bootstrap process.

Before creating a new bootstrap project, inspect the existing solution.

Choose the simplest approach compatible with the current architecture.

Possible options:

1. Console project
2. CLI command
3. PowerShell script
4. Dedicated API endpoint

Prefer:

A separate .NET console/bootstrap project

if it fits naturally into the existing solution.

The bootstrap process should:

1. Connect to Cosmos DB Emulator vNext.
2. Verify connectivity.
3. Create the database if it does not exist.
4. Create the container if it does not exist.
5. Generate approximately 5,000 documents.
6. Insert the documents efficiently.
7. Display progress.
8. Display a final summary.

For example:

Database created or already exists.

Container created or already exists.

Generating 5,000 sample documents.

Inserted 1,000 / 5,000

Inserted 2,000 / 5,000

...

Bootstrap completed successfully.

---

# DATA INSERTION

Do not insert 5,000 documents inefficiently one-by-one if a better supported approach exists.

Investigate appropriate Cosmos DB SDK mechanisms.

Use an approach that is:

- Reliable
- Understandable
- Suitable for a learning project

If concurrency is used:

- Limit concurrency
- Avoid overwhelming the emulator
- Make the concurrency configurable

Do not create excessive parallel operations.

---

# RESET PROCESS

Create a reset process.

The reset process should work at any time.

The process should:

1. Only affect this sample project's database/container.
2. Remove existing sample data.
3. Recreate the required data.
4. Generate approximately 5,000 documents.
5. Complete successfully when run repeatedly.

The reset process should be:

Idempotent.

Document exactly what reset does.

Do not delete unrelated databases or containers.

---

# CRUD API

Analyze existing CRUD endpoints first.

Reuse them where possible.

The final API should support:

## CREATE

POST

Create a document.

---

## READ

GET

Get a document by:

- id
- partition key

Use a Cosmos DB point read.

This is important.

Clearly demonstrate the difference between:

Point Read

and:

Query

---

## LIST

GET

Retrieve multiple documents.

Use pagination where appropriate.

---

## UPDATE

Use the approach already used in the project where appropriate.

Possible options:

PUT

for replace.

Also consider:

PATCH

to demonstrate Cosmos DB Patch operations.

Do not implement PATCH unnecessarily if it makes the project significantly more complicated.

---

## DELETE

DELETE

Delete a document.

Use:

- id
- partition key

---

# HTTP QUERY METHOD

This is a primary learning objective.

IMPORTANT:

Do not assume HTTP QUERY method support.

Before implementing:

1. Inspect the actual .NET version.
2. Inspect the ASP.NET Core version.
3. Research/verify the actual HTTP method support available.
4. Verify routing behavior.
5. Verify OpenAPI behavior.

The goal is to implement an actual:

QUERY

HTTP method.

Do not silently replace it with:

POST

and call it QUERY.

If the framework supports a custom HTTP method:

Implement it properly.

For example conceptually:

[HttpMethod("QUERY")]

or the correct modern ASP.NET Core approach.

However:

Do not use this example blindly.

Use the actual supported API.

---

# IF HTTP QUERY HAS TOOLING LIMITATIONS

Swagger/OpenAPI tools may not fully support custom HTTP methods.

If that happens:

1. Keep the actual QUERY endpoint.
2. Do not replace it with POST.
3. Document the limitation.
4. Provide alternative testing options.

Create:

scripts/sample-requests/api.http

Include actual HTTP QUERY examples.

Also provide:

curl examples.

For example conceptually:

curl -X QUERY ...

Use the actual endpoint and request format implemented by the project.

---

# QUERY API DESIGN

Do not expose arbitrary Cosmos SQL directly to normal API users.

Create a structured query request model.

The API should support fields such as:

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
- tags
- city

Include:

- sorting
- pageSize
- continuationToken

The exact model should be designed appropriately.

---

# QUERY REQUEST EXAMPLE

Conceptually:

{
    "category": "Electronics",
    "status": "Active",
    "minimumPrice": 500,
    "maximumPrice": 5000,
    "minimumRating": 4,
    "isActive": true,
    "tags": [
        "wireless",
        "premium"
    ],
    "sortField": "price",
    "sortDirection": "ascending",
    "pageSize": 20,
    "continuationToken": null
}

This is only an example.

Improve the model if necessary.

---

# PARAMETERIZED QUERIES

This is mandatory.

Do not build Cosmos DB SQL by concatenating user input.

Use:

QueryDefinition

and:

WithParameter

or the appropriate Cosmos SDK mechanism.

Example conceptually:

SELECT * FROM c
WHERE c.category = @category

Do not concatenate:

"... WHERE c.category = '" + category + "'"

---

# QUERY SCENARIOS

The API should support or demonstrate the following.

## 1. Simple Filtering

Examples:

- category
- status
- isActive

---

## 2. Numeric Queries

Examples:

- Greater than
- Less than
- Range

Example:

price >= 1000

---

## 3. Multiple Conditions

Example:

Category

AND

Status

AND

Price range

---

## 4. Boolean Queries

Examples:

isActive = true

isFeatured = true

---

## 5. Date Queries

Examples:

createdAfter

createdBefore

Date ranges.

---

## 6. Sorting

Support safe sorting.

Do not directly insert arbitrary client-provided SQL into ORDER BY.

Use an allow-list of supported sort fields.

Examples:

- price
- rating
- createdAt
- productName

Validate:

ascending

descending

---

## 7. Null Queries

Demonstrate Cosmos DB behavior for:

- null values
- missing properties

Document the difference.

---

## 8. Nested Object Queries

Examples:

manufacturer.country

warehouseLocation.city

metadata.source

---

## 9. Array Queries

Examples:

tags

supportedCountries

Use appropriate Cosmos DB SQL functions.

---

## 10. Nested Array Queries

Demonstrate querying:

reviews

or another nested array.

Use appropriate Cosmos DB query syntax.

---

## 11. Complex Query

Create realistic combinations.

For example:

Electronics

AND

Active

AND

Price between 1000 and 5000

AND

Rating greater than 4

AND

Contains a specific tag.

---

# PAGINATION

Demonstrate Cosmos DB pagination correctly.

Use continuation tokens.

The API response should contain:

- items
- continuationToken
- count

Do not implement OFFSET/LIMIT as the primary pagination mechanism.

The continuation token should be passed back to the API to retrieve the next page.

---

# QUERY RESPONSE

Create a useful response model.

Example:

{
    "items": [],
    "count": 20,
    "continuationToken": "...",
    "requestCharge": 3.45,
    "activityId": "..."
}

Include Cosmos DB metadata where available and useful.

Do not expose secrets.

---

# COSMOS DB EDUCATIONAL FEATURES

Where practical, demonstrate:

1. Point Reads
2. Queries
3. Partition Keys
4. Parameterized Queries
5. Continuation Tokens
6. ETags
7. Optimistic Concurrency
8. Patch Operations
9. Request Charge
10. Activity ID
11. Cosmos DB Exceptions

Do not over-engineer.

Prioritize educational value.

---

# EXCEPTION HANDLING

Reuse the existing exception handling mechanism.

If none exists, add a simple consistent approach.

Handle Cosmos DB exceptions.

Examples:

404

Not Found.

409

Conflict.

400

Bad Request.

429

Request Rate Too Large.

500

Unexpected error.

Do not expose stack traces to clients.

---

# LOGGING

Use structured logging.

Log:

- Application startup
- Cosmos connection status
- Bootstrap progress
- Reset progress
- Query execution
- Cosmos errors

Do not log:

- Account keys
- Secrets

---

# SWAGGER / OPENAPI

Keep Swagger/OpenAPI enabled if already configured.

Document:

- CRUD endpoints
- Request models
- Response models

For the HTTP QUERY method:

Verify actual Swagger/OpenAPI support.

If QUERY does not appear correctly:

Document the limitation.

Provide:

api.http

and:

curl

examples.

Do not compromise the HTTP method just to make Swagger display it.

---

# SAMPLE HTTP REQUEST FILE

Create or update:

scripts/sample-requests/api.http

Include:

## CRUD

Create.

Get by ID.

List.

Update.

Delete.

---

## QUERY

Simple query.

Numeric query.

Multiple conditions.

Date query.

Nested object query.

Array query.

Complex query.

Pagination.

Each request should contain a comment explaining what it demonstrates.

---

# TESTING

Inspect existing tests first.

Do not add a complex testing framework unnecessarily.

Add useful tests for:

- Query request validation
- Sort field validation
- Query builder logic
- Important business logic

Integration tests are optional.

If implemented, they must be documented.

---

# README.md

Update the README completely.

The README must be written for a developer starting from scratch.

---

# README SECTION 1 — Project Overview

Explain:

- Purpose of the project
- Learning objectives
- Cosmos DB features demonstrated

---

# README SECTION 2 — Architecture

Include:

Client

↓

ASP.NET Core Web API

↓

Microsoft.Azure.Cosmos SDK

↓

Azure Cosmos DB Emulator vNext

↓

Docker

---

# README SECTION 3 — Prerequisites

Document:

- Windows
- Docker Desktop
- WSL 2
- .NET 10 SDK
- Git
- Cosmos DB Emulator vNext

---

# README SECTION 4 — Start Cosmos DB Emulator

Provide the exact Docker instructions.

Include:

docker ps

and:

docker logs cosmos-vnext --tail 50

Document successful output.

For example:

PostgreSQL=OK

Gateway=OK

Explorer=OK

and:

Now listening on:

https://0.0.0.0:8081

---

# README SECTION 5 — Certificate Setup

This is important.

Provide separate instructions.

## Windows PowerShell 5.1

Use:

curl.exe -k https://localhost:8081/_explorer/emulator.pem -o emulatorcert.crt

Then:

Import-Certificate `
  -FilePath .\emulatorcert.crt `
  -CertStoreLocation Cert:\CurrentUser\Root

Explain:

Windows PowerShell 5.1 does not support:

SkipCertificateCheck

for Invoke-WebRequest.

---

## PowerShell 7+

Keep the PowerShell 7+ method.

For example:

$parameters = @{
    Uri = 'https://localhost:8081/_explorer/emulator.pem'
    Method = 'GET'
    OutFile = 'emulatorcert.crt'
    SkipCertificateCheck = $True
}

Invoke-WebRequest @parameters

Then import the certificate.

---

# README SECTION 6 — Clone Project

Provide:

git clone

cd

---

# README SECTION 7 — Restore and Build

Provide:

dotnet restore

dotnet build

---

# README SECTION 8 — Configuration

Explain:

- Cosmos endpoint
- Account key
- Database name
- Container name

Do not expose real secrets.

Explain where configuration is stored.

---

# README SECTION 9 — Bootstrap

Provide exact instructions.

Explain:

What happens:

1. Database created.
2. Container created.
3. Approximately 5,000 documents generated.
4. Documents inserted.

Provide verification steps.

---

# README SECTION 10 — Run API

Provide exact commands.

Explain:

API URL.

Swagger URL.

Any limitations for HTTP QUERY in Swagger.

---

# README SECTION 11 — CRUD Examples

Provide examples.

---

# README SECTION 12 — HTTP QUERY Examples

This should be a detailed section.

Start simple.

Then progressively demonstrate:

1. Category filter.
2. Price range.
3. Multiple conditions.
4. Date range.
5. Nested object.
6. Array.
7. Complex query.
8. Pagination.

Explain each example.

---

# README SECTION 13 — Reset Data

Provide the exact reset command.

Explain:

The reset process only affects this sample project's Cosmos DB database/container.

---

# README SECTION 14 — Project Structure

Explain important folders.

---

# README SECTION 15 — Troubleshooting

Include:

## Docker is not running.

## Emulator is not running.

## Port 8081 is already in use.

## Container name already exists.

For example:

docker rm -f cosmos-vnext

## Certificate errors.

## PowerShell 5.1 SkipCertificateCheck error.

## Cosmos DB connection error.

## Bootstrap fails.

## API cannot connect to emulator.

Include practical diagnostic commands.

---

# DO NOT OVER-ENGINEER

Avoid unnecessary:

- Generic repository patterns
- Excessive interfaces
- Complex CQRS
- MediatR unless already used
- Microservices
- Event sourcing
- Multiple unnecessary projects

Use the existing architecture.

The goal is:

A clear, working, educational Cosmos DB learning project.

---

# BUILD AND VERIFY

After implementation:

Run:

dotnet restore

dotnet build

Fix all compilation errors.

Run tests if available.

Verify:

1. Bootstrap works.
2. Approximately 5,000 documents are created.
3. Reset works.
4. API starts.
5. Cosmos DB connection works.
6. CRUD operations work.
7. Point read works.
8. QUERY method works.
9. Pagination works.

Do not claim verification if it was not actually performed.

---

# FINAL RESPONSE

After implementation, provide:

## 1. Summary

What was implemented.

---

## 2. Existing Components Reused

List them.

---

## 3. Files Created

List them.

---

## 4. Files Modified

List them.

---

## 5. Bootstrap

Provide the exact command.

---

## 6. Reset

Provide the exact command.

---

## 7. Run API

Provide the exact command.

---

## 8. Test QUERY

Provide an example.

---

## 9. Verification

Clearly state:

- What was successfully built.
- What was tested.
- What could not be tested.
- Any limitations.

Do not claim 100% verification unless it was actually performed.

---

# FINAL ACCEPTANCE CRITERIA

The work is complete only when:

✓ Existing project was analyzed first.

✓ Existing functionality was preserved.

✓ Changes were incremental.

✓ Project builds successfully.

✓ Cosmos DB Emulator vNext is supported.

✓ Configuration is clean.

✓ Bootstrap creates approximately 5,000 documents.

✓ Data contains meaningful variation.

✓ Reset works.

✓ CRUD works.

✓ Point reads are demonstrated.

✓ Actual HTTP QUERY method is implemented and verified if supported by the framework.

✓ Parameterized Cosmos DB queries are used.

✓ Sorting is protected using an allow-list.

✓ Arrays are demonstrated.

✓ Nested objects are demonstrated.

✓ Null behavior is demonstrated.

✓ Complex queries are demonstrated.

✓ Continuation tokens are demonstrated.

✓ Sample HTTP requests are available.

✓ README is updated.

✓ Windows PowerShell 5.1 certificate instructions use curl.exe.

✓ PowerShell 7+ instructions remain available.

✓ No unrelated functionality was removed.

✓ No unnecessary over-engineering was introduced.