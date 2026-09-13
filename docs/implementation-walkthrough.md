# Implementation Walkthrough — Cosmos DB Hybrid Search

## 1. Goal

This guide explains how to reproduce the hybrid product-search demonstration on a local Windows machine.

The implementation uses:

- .NET 10
- ASP.NET Core Web API
- Azure Cosmos DB vNext Linux Docker Emulator
- Azure Cosmos DB .NET SDK
- Ollama
- `mxbai-embed-large`
- Cosmos DB vector search with DiskANN
- Application-side BM25-style keyword ranking
- Weighted Reciprocal Rank Fusion

No cloud Cosmos DB account or cloud AI service is required for this local demonstration.

## 2. Prerequisites

Install or have available:

1. Windows with Docker Desktop
2. .NET 10 SDK
3. Git, if cloning the project
4. Ollama
5. Postman or `curl.exe`

Verify:

```powershell
dotnet --version
docker --version
ollama --version
```

## 3. Start Ollama

Install/pull the embedding model:

```powershell
ollama pull mxbai-embed-large
```

Verify:

```powershell
ollama list
```

The application uses:

```text
http://localhost:11434/api/embed
```

and expects 1024-dimensional embeddings.

## 4. Start the Cosmos DB vNext Emulator

Start the Linux vNext Cosmos DB Emulator using the existing Docker configuration for the project.

The application expects:

```text
https://localhost:8081/
```

Make sure the emulator is running before starting the API.

Use the project's known emulator setup rather than introducing a different configuration.

## 5. Obtain the Project

If using source control:

```powershell
git clone <repository-url>
cd <repository-folder>
```

Or open the existing solution/project directory.

## 6. Restore and Build

From the API project directory:

```powershell
dotnet restore
dotnet build
```

The project was developed with .NET 10 and uses the Cosmos SDK resolved as:

```text
Microsoft.Azure.Cosmos 3.64.0P
```

The build must succeed before continuing.

## 7. Verify Configuration

The logical configuration is:

```text
Cosmos endpoint:
    https://localhost:8081/

Database:
    VectorDemoDb

Hybrid container:
    HybridProducts

Partition key:
    /category

Ollama:
    http://localhost:11434
```

Do not point the hybrid feature at the existing `Products` container.

The hybrid feature uses:

```text
HybridProducts
```

## 8. Project Structure

A representative structure is:

```text
CosmosLearning.Vector.Api/
│
├── Features/
│   ├── VectorSearch/
│   │   └── ...
│   │
│   └── HybridSearch/
│       ├── HybridSearchController.cs
│       ├── HybridSearchRequest.cs
│       ├── HybridSearchResult.cs
│       ├── HybridSearchMode.cs
│       ├── HybridSearchService.cs
│       ├── KeywordSearchService.cs
│       └── ReciprocalRankFusion.cs
│
├── Infrastructure/
│   └── CosmosDb/
│       ├── CosmosHybridRepository.cs
│       ├── CosmosHybridDatabaseInitializer.cs
│       └── ...
│
├── LargeProductCatalog.cs
├── Program.cs
└── appsettings.json
```

## 9. Container Configuration

The application initializer creates/configures:

```text
Database:
    VectorDemoDb

Container:
    HybridProducts

Partition key:
    /category

Vector path:
    /embedding

Dimensions:
    1024

Data type:
    float32

Distance:
    cosine

Vector index:
    DiskANN

Search text:
    /searchText
```

A full-text policy/index is also configured on `/searchText`.

## 10. Start the API

Run:

```powershell
dotnet run
```

The local HTTP API used by this project runs at:

```text
http://localhost:5142
```

The HTTPS API is not required for this local demonstration.

Keep the API running.

## 11. Verify Database Initialization

On startup, the hybrid database initializer should ensure:

```text
VectorDemoDb
    └── HybridProducts
```

exists with the required configuration.

If the container already exists, it is reused.

## 12. Seed the Hybrid Catalog

Use:

```powershell
curl.exe -X POST "http://localhost:5142/api/hybrid/seed?count=1000"
```

Expected response resembles:

```json
{
  "requested": 1000,
  "inserted": 1000,
  "message": "Hybrid search catalog has been embedded with Ollama and stored in Cosmos DB."
}
```

If documents already exist, `inserted` can be less than `requested`, because the operation targets the requested total.

## 13. What Happens During Seeding

```text
Seed request
     │
     ▼
Count existing HybridProducts
     │
     ▼
Generate missing products
     │
     ▼
Build SearchText
     │
     ▼
Send text to Ollama
     │
     ▼
Receive 1024-dimensional embedding
     │
     ▼
Create Cosmos DB document
     │
     ▼
Store id/name/category/price/description/
searchText/embedding
```

Products are embedded in batches before insertion.

## 14. Verify Stored Data

Use the Cosmos DB emulator data explorer or another suitable Cosmos DB client.

Confirm:

```text
VectorDemoDb
    └── HybridProducts
```

A document should contain:

```json
{
  "id": "...",
  "name": "...",
  "category": "...",
  "price": 10000,
  "description": "...",
  "searchText": "...",
  "embedding": [ ... ]
}
```

The embedding should contain 1024 values.

## 15. First Hybrid Search

Request:

```http
POST http://localhost:5142/api/hybrid/search
Content-Type: application/json
```

Body:

```json
{
  "query": "something for esports with very fast response",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 1,
  "maximumPrice": 20000,
  "top": 5
}
```

The expected product family is esports/rapid mechanical keyboards.

This demonstrates semantic intent plus lexical matching.

## 16. Exact Technical Search

Use:

```json
{
  "query": "8000Hz 0.5ms anti-ghosting",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 1,
  "maximumPrice": 20000,
  "top": 5
}
```

Expected behavior:

- Exact technical terms strongly influence keyword ranking.
- Semantically related gaming keyboards appear in vector candidates.
- RRF combines the two ranked lists.

An observed result was:

```text
Esports Rapid Mechanical Keyboard 2
Vector Rank: 30
Keyword Rank: 2
RRF Score: 0.02724014336917563
```

## 17. Understand the RRF Calculation

For:

```text
Vector Rank = 30
Keyword Rank = 2
Vector Weight = 1
Keyword Weight = 1
Rank Constant = 60
```

the calculation is:

```text
Vector Contribution
= 1 / (60 + 30)
= 0.011111...

Keyword Contribution
= 1 / (60 + 2)
= 0.016129...

RRF
= 0.011111...
  + 0.016129...

= 0.027240...
```

## 18. Semantic Paraphrase Test

Use:

```json
{
  "query": "a keyboard for someone who types code all day",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 1,
  "maximumPrice": 10000,
  "top": 5
}
```

This demonstrates that vector search can recognize coding/productivity intent without requiring the exact query phrase.

Ergonomic productivity keyboards should rank strongly.

## 19. Office Intent Test

Use:

```json
{
  "query": "quiet keyboard for an office shared workspace",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 1,
  "maximumPrice": 10000,
  "top": 5
}
```

The expected product family is:

```text
Silent Office Keyboard
```

## 20. Vector-Only Mode

Use:

```json
{
  "query": "keyboard for competitive gaming",
  "mode": "vector",
  "maximumPrice": 20000,
  "top": 5
}
```

The results should expose `vectorRank` while `keywordRank` is null.

## 21. Keyword-Only Mode

Use:

```json
{
  "query": "8000Hz 0.5ms anti-ghosting",
  "mode": "keyword",
  "maximumPrice": 20000,
  "top": 5
}
```

The results should expose `keywordRank` while `vectorRank` is null.

## 22. Balanced Hybrid

Use:

```json
{
  "query": "8000Hz 0.5ms anti-ghosting",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 1,
  "maximumPrice": 20000,
  "top": 5
}
```

This is the default balanced configuration.

## 23. Vector-Heavy Hybrid

Use:

```json
{
  "query": "8000Hz 0.5ms anti-ghosting",
  "mode": "hybrid",
  "vectorWeight": 2,
  "keywordWeight": 1,
  "maximumPrice": 20000,
  "top": 5
}
```

The final ranking should move toward documents with stronger semantic/vector ranks.

## 24. Keyword-Heavy Hybrid

Use:

```json
{
  "query": "8000Hz 0.5ms anti-ghosting",
  "mode": "hybrid",
  "vectorWeight": 1,
  "keywordWeight": 2,
  "maximumPrice": 20000,
  "top": 5
}
```

Exact lexical matching now has greater influence.

This is useful for technical specifications, model numbers, product codes, and exact feature names.

## 25. Metadata Filtering

Category:

```json
{
  "query": "gaming keyboard",
  "mode": "hybrid",
  "category": "Gaming",
  "top": 5
}
```

Price:

```json
{
  "query": "gaming keyboard",
  "mode": "hybrid",
  "minimumPrice": 5000,
  "maximumPrice": 20000,
  "top": 5
}
```

Both:

```json
{
  "query": "gaming keyboard",
  "mode": "hybrid",
  "category": "Gaming",
  "minimumPrice": 5000,
  "maximumPrice": 20000,
  "top": 5
}
```

## 26. Verify Search Explanation

Every search response should contain:

```json
"searchExplanation": {
  "algorithm": "Weighted Reciprocal Rank Fusion",
  "rankConstant": 60,
  "vectorCandidateCount": 100,
  "vectorWeight": 1,
  "keywordWeight": 1,
  "vectorSearch": "Cosmos DB VectorDistance with DiskANN",
  "keywordSearch": "Application-side BM25-style lexical ranking",
  "fusion": "Weighted RRF combines vector and keyword ranks"
}
```

Verify:

```text
algorithm = Weighted Reciprocal Rank Fusion
rankConstant = 60
vectorCandidateCount = 100
```

## 27. Verify Ranking Diagnostics

Inspect result objects such as:

```json
{
  "name": "Esports Rapid Mechanical Keyboard 2",
  "vectorRank": 30,
  "keywordRank": 2,
  "vectorContribution": 0.011111111111111112,
  "keywordContribution": 0.016129032258064516,
  "rrfScore": 0.02724014336917563
}
```

Verify that:

```text
rrfScore =
    vectorContribution
    +
    keywordContribution
```

when both weights are 1.

## 28. Verify Candidate Window

The hybrid service retrieves:

```text
100 vector candidates
```

and returns:

```text
Top 5
```

This is visible in:

```json
"vectorCandidateCount": 100
```

The value is intentionally larger than `top` so that RRF has a useful semantic candidate pool.

## 29. Technical Tokenization

The keyword tokenizer preserves technical product tokens such as:

```text
8000Hz
0.5ms
2.4GHz
240Hz
1TB
32GB
26000DPI
```

This is important because simplistic word tokenization can destroy the meaning of product specifications.

## 30. Reset and Reseed

To clear the hybrid catalog:

```powershell
curl.exe -X DELETE "http://localhost:5142/api/hybrid/reset"
```

Then seed again:

```powershell
curl.exe -X POST "http://localhost:5142/api/hybrid/seed?count=1000"
```

The reset targets only:

```text
HybridProducts
```

It does not reset the original `Products` vector-search container.

## 31. Recommended Verification Sequence

```text
1. Start Ollama.
2. Verify mxbai-embed-large.
3. Start the Cosmos DB vNext Emulator.
4. Verify https://localhost:8081/.
5. Run dotnet restore.
6. Run dotnet build.
7. Start the API with dotnet run.
8. Seed HybridProducts with 1,000 products.
9. Run balanced hybrid search.
10. Run vector-only search.
11. Run keyword-only search.
12. Run vector-heavy RRF.
13. Run keyword-heavy RRF.
14. Test category/price filters.
15. Inspect ranking diagnostics.
16. Inspect searchExplanation.
```

## 32. Troubleshooting

### Cosmos connection failure

Verify the emulator is running at:

```text
https://localhost:8081/
```

and that the API uses the same endpoint.

### Ollama embedding failure

Verify:

```powershell
ollama list
```

and ensure:

```text
mxbai-embed-large
```

is installed.

Verify Ollama is available at:

```text
http://localhost:11434
```

### No search results

Check:

1. `HybridProducts` contains documents.
2. Category filters are correct.
3. Price ranges are not too restrictive.
4. The query is not empty.
5. The API points to the intended emulator/database.

### `Count` or indexing build error

`LargeProductCatalog.Generate(...)` returns an enumerable. Where list semantics are required:

```csharp
var products =
    LargeProductCatalog
        .Generate(productsToCreate)
        .ToList();
```

Do not redesign the repository solely for this issue.

### `Document does not contain an id field`

The Cosmos SDK used by this project uses Newtonsoft.Json serialization. Cosmos document models should use the appropriate Newtonsoft attribute, for example:

```csharp
[JsonProperty("id")]
public string Id { get; set; }
```

Do not rely only on `System.Text.Json` attributes for Cosmos SDK document serialization.

### `FullTextScore` error

If the emulator returns:

```text
SC2005: 'FullTextScore' is not a recognized built-in function name.
```

this is the local emulator limitation encountered by this project.

The correct local architecture remains:

```text
Vector:
    Cosmos VectorDistance + DiskANN

Keyword:
    Application-side BM25-style ranking

Fusion:
    Application-side Weighted RRF
```

## 33. Final Validation Checklist

```text
[ ] Ollama installed
[ ] mxbai-embed-large installed
[ ] Cosmos DB vNext Emulator running
[ ] .NET 10 installed
[ ] dotnet restore succeeds
[ ] dotnet build succeeds
[ ] API starts
[ ] VectorDemoDb exists
[ ] HybridProducts exists
[ ] HybridProducts uses /category
[ ] /embedding is 1024-dimensional float32 cosine
[ ] DiskANN vector index exists
[ ] Hybrid catalog seeded
[ ] Vector-only search works
[ ] Keyword-only search works
[ ] Hybrid search works
[ ] Metadata filtering works
[ ] Weighted RRF works
[ ] Vector weight changes ranking
[ ] Keyword weight changes ranking
[ ] Ranking diagnostics are returned
[ ] Search explanation is returned
[ ] Hybrid reset works
```

## 34. What Has Been Demonstrated

```text
Ollama
  ↓
Embeddings
  ↓
Cosmos DB Vector Search
  ↓
DiskANN

Query
  ↓
Keyword Tokenization
  ↓
BM25-style Ranking

Vector Rank + Keyword Rank
  ↓
Weighted RRF
  ↓
Final Hybrid Ranking
```

The implementation demonstrates structured filters and explainable ranking in addition to hybrid retrieval.

## 35. Key Takeaway

```text
Vector search answers:
"What is this query conceptually about?"

Keyword search answers:
"Which documents contain the important exact terms?"

RRF answers:
"How should the two ranked lists be combined?"
```

For this local emulator project, the architecture is implemented using Cosmos DB vector search plus application-side lexical ranking and Weighted RRF.
