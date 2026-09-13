# Technical Design — Cosmos DB Vector + Keyword Hybrid Search

## 1. Purpose

This document describes the hybrid product-search capability implemented in the local Cosmos DB vNext Emulator learning project.

The implementation combines:

- Local Ollama embeddings
- Azure Cosmos DB vector search using DiskANN
- Application-side BM25-style lexical ranking
- Weighted Reciprocal Rank Fusion (RRF)
- Structured metadata filtering
- Ranking diagnostics and search-explanation metadata

The implementation is deliberately compatible with the local Cosmos DB vNext Emulator used by this project.

## 2. Scope

The hybrid capability uses:

- Database: `VectorDemoDb`
- Container: `HybridProducts`
- Partition key: `/category`
- Vector path: `/embedding`
- Search text path: `/searchText`
- Embedding dimensions: `1024`
- Vector type: `float32`
- Distance function: `cosine`
- Vector index: `DiskANN`
- Embedding model: Ollama `mxbai-embed-large`

The existing `Products` container remains the separate vector-search demonstration and is not modified by the hybrid reset endpoint.

## 3. High-Level Architecture

```text
                         ┌─────────────────────┐
                         │      Client         │
                         │ Postman / curl      │
                         └──────────┬──────────┘
                                    │
                                    │ POST /api/hybrid/search
                                    ▼
                    ┌──────────────────────────────┐
                    │ HybridSearchController       │
                    │ ASP.NET Core Web API         │
                    └──────────────┬───────────────┘
                                   │
                                   ▼
                    ┌──────────────────────────────┐
                    │ HybridSearchService           │
                    │                              │
                    │ • validates request          │
                    │ • selects search mode        │
                    │ • coordinates retrieval      │
                    │ • invokes RRF                │
                    └──────────────┬───────────────┘
                                   │
                     ┌─────────────┴─────────────┐
                     │                           │
                     ▼                           ▼
          ┌─────────────────────┐      ┌──────────────────────┐
          │ Vector Search       │      │ Keyword Search       │
          │                     │      │                      │
          │ Cosmos DB           │      │ Application-side     │
          │ VectorDistance      │      │ BM25-style ranking   │
          │ + DiskANN           │      │                      │
          └──────────┬──────────┘      └───────────┬──────────┘
                     │                             │
                     │ Vector Rank                 │ Keyword Rank
                     └─────────────┬───────────────┘
                                   ▼
                    ┌──────────────────────────────┐
                    │ Reciprocal Rank Fusion       │
                    │ Weighted RRF                 │
                    │ rankConstant = 60            │
                    └──────────────┬───────────────┘
                                   │
                                   ▼
                    ┌──────────────────────────────┐
                    │ Final Ranked Results          │
                    │ + diagnostics                │
                    │ + search explanation         │
                    └──────────────────────────────┘
```

## 4. End-to-End Search Flow

### Step 1 — Query

Example:

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

### Step 2 — Embedding

The application sends the query to local Ollama:

```text
http://localhost:11434/api/embed
```

using:

```text
mxbai-embed-large
```

The model produces a 1024-dimensional embedding.

### Step 3 — Vector retrieval

The embedding is supplied to Cosmos DB.

The repository executes vector similarity search using `VectorDistance` and the configured DiskANN index.

The hybrid service requests:

```text
Vector candidate count = 100
```

even though the API normally returns five results. The larger candidate pool gives RRF more documents to consider.

### Step 4 — Keyword retrieval

The same query is tokenized by `KeywordSearchService`.

The service performs application-side BM25-style lexical ranking over documents satisfying the structured filters.

The tokenizer preserves technical tokens such as:

```text
8000Hz
0.5ms
2.4GHz
240Hz
1TB
32GB
26000DPI
```

This is important for exact product specifications.

### Step 5 — Weighted RRF

The two ranked lists are fused.

The formula is:

```text
Contribution = Weight / (RankConstant + Rank)
```

The implementation uses:

```text
RankConstant = 60
```

and:

```text
Final RRF =
    Vector Contribution
    + Keyword Contribution
```

For example:

```text
Vector Rank  = 30
Keyword Rank = 2
Vector Weight = 1
Keyword Weight = 1
```

produces:

```text
Vector Contribution
= 1 / (60 + 30)
= 0.011111...

Keyword Contribution
= 1 / (60 + 2)
= 0.016129...

Final RRF
= 0.027240...
```

This matches the observed API response.

## 5. Why Hybrid Search

Vector and keyword retrieval solve different relevance problems.

### Vector search

Vector search is strong at semantic intent and paraphrasing.

Example:

```text
a keyboard for someone who types code all day
```

can retrieve ergonomic/productivity keyboards even when the exact phrase does not occur in a document.

### Keyword search

Keyword search is strong when exact terms matter.

Example:

```text
8000Hz 0.5ms anti-ghosting
```

contains technical specifications where exact lexical matching is valuable.

### Hybrid search

The combined model is:

```text
Semantic relevance
       +
Exact lexical relevance
       =
More robust ranking
```

## 6. Supported Search Modes

### Vector

```json
{
  "query": "keyboard for competitive gaming",
  "mode": "vector"
}
```

Only vector ranking is used.

### Keyword

```json
{
  "query": "anti-ghosting mechanical keyboard",
  "mode": "keyword"
}
```

Only application-side lexical ranking is used.

### Hybrid

```json
{
  "query": "anti-ghosting mechanical keyboard",
  "mode": "hybrid"
}
```

Both retrieval strategies participate and RRF combines their ranks.

## 7. Weighted RRF

Weights control the relative importance of the two signals.

Balanced:

```json
{
  "vectorWeight": 1,
  "keywordWeight": 1
}
```

Vector-heavy:

```json
{
  "vectorWeight": 2,
  "keywordWeight": 1
}
```

Keyword-heavy:

```json
{
  "vectorWeight": 1,
  "keywordWeight": 2
}
```

Vector-heavy configurations favor semantic similarity. Keyword-heavy configurations are useful for technical specifications, model numbers, product codes, and exact feature names.

## 8. Metadata Filtering

The request supports:

- `category`
- `minimumPrice`
- `maximumPrice`

Example:

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

Structured filtering therefore works together with semantic and lexical retrieval.

## 9. Data Model

Conceptually, a hybrid product document contains:

```text
Product
 ├── id
 ├── name
 ├── category
 ├── price
 ├── description
 ├── searchText
 └── embedding[]
```

`searchText` contains meaningful searchable information such as product name, description, features, use cases, keywords, and relevant technical specifications.

The embedding represents semantic content.

The search text supports lexical retrieval.

## 10. Cosmos DB Container Design

`HybridProducts` uses:

```text
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
```

A full-text policy/index is also configured on:

```text
/searchText
```

However, the local emulator build used by this project does not support the `FullTextScore()` query function required by the attempted native hybrid query.

## 11. Important Local Emulator Limitation

The project initially attempted a native Cosmos hybrid query using the concept:

```sql
ORDER BY RANK RRF(
    VectorDistance(c.embedding, @embedding),
    FullTextScore(c.searchText, @searchText)
)
```

The local emulator returned:

```text
SC2005: 'FullTextScore' is not a recognized built-in function name.
```

Therefore this project does not claim that the local emulator is executing native Cosmos full-text hybrid ranking.

Instead, the local architecture is:

```text
Cosmos DB:
    VectorDistance + DiskANN

Application:
    BM25-style keyword ranking
    Weighted RRF
```

This distinction should remain explicit in architecture documentation.

## 12. Candidate Window

The hybrid service retrieves:

```text
100 vector candidates
```

and normally returns:

```text
5 results
```

A broader vector candidate window prevents potentially useful documents from being discarded before the keyword list and RRF can influence the final ranking.

The value is a demo tuning choice, not a universal production recommendation.

## 13. Ranking Diagnostics

Each result exposes:

```text
vectorRank
keywordRank
vectorContribution
keywordContribution
rrfScore
```

Example:

```json
{
  "vectorRank": 30,
  "keywordRank": 2,
  "vectorContribution": 0.011111111111111112,
  "keywordContribution": 0.016129032258064516,
  "rrfScore": 0.02724014336917563
}
```

This makes the final ranking explainable.

## 14. Search Explanation

The API returns:

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

This is useful for learning, debugging, demonstrations, and architecture reviews.

## 15. Application Components

The implementation is organized by feature.

```text
Features/
└── HybridSearch/
    ├── HybridSearchController.cs
    ├── HybridSearchRequest.cs
    ├── HybridSearchResult.cs
    ├── HybridSearchMode.cs
    ├── HybridSearchService.cs
    ├── KeywordSearchService.cs
    └── ReciprocalRankFusion.cs
```

Cosmos infrastructure:

```text
Infrastructure/
└── CosmosDb/
    ├── CosmosHybridRepository.cs
    ├── CosmosHybridDatabaseInitializer.cs
    └── ...
```

Responsibilities:

- Controller: HTTP concerns
- Service: orchestration and validation
- Repository: Cosmos DB operations
- Keyword service: lexical ranking
- RRF: rank fusion
- Initializer: database/container configuration

## 16. Seeding

The search-oriented catalog contains differentiated product families including:

- esports mechanical keyboards
- low-latency gaming keyboards
- silent office keyboards
- ergonomic productivity keyboards
- developer laptops
- AI development laptops
- gaming mice
- monitors
- SSDs
- headphones
- cameras
- office chairs
- standing desks
- fitness watches

Products are generated with meaningful search text and embedded through Ollama before being stored in Cosmos DB.

The seed operation checks the existing count and creates only enough documents to reach the requested target.

## 17. Seed Flow

```text
POST /api/hybrid/seed?count=1000
             │
             ▼
     Hybrid Repository
             │
             ▼
 Generate missing products
             │
             ▼
        Ollama Embed
             │
             ▼
       1024-d vector
             │
             ▼
       Cosmos DB
      HybridProducts
```

## 18. Reset Flow

```text
DELETE /api/hybrid/reset
             │
             ▼
     Read product IDs
             │
             ▼
 Delete documents using
 their category partition key
             │
             ▼
        HybridProducts
```

The reset endpoint only affects `HybridProducts`, not the original `Products` container.

## 19. Demonstration Scenarios

### Semantic paraphrase

```text
a keyboard for someone who types code all day
```

Demonstrates semantic intent and paraphrase handling.

### Exact technical query

```text
8000Hz 0.5ms anti-ghosting
```

Demonstrates the value of exact technical keyword retrieval combined with semantic vector retrieval.

### Office intent

```text
quiet keyboard for an office shared workspace
```

Demonstrates differentiated semantic concepts and lexical overlap.

## 20. Observed Result

For:

```text
8000Hz 0.5ms anti-ghosting
```

an observed result was:

```text
Esports Rapid Mechanical Keyboard 2

Vector Rank:
30

Keyword Rank:
2

Vector Contribution:
0.011111111111111112

Keyword Contribution:
0.016129032258064516

RRF Score:
0.02724014336917563
```

This demonstrates that a document does not need to rank first in both systems. A strong rank in one retrieval system can compensate for a weaker rank in the other.

## 21. Benefits

- Clear separation of concerns
- Explainable ranking
- Configurable vector/keyword weighting
- Local development without cloud AI
- Compatibility with the tested emulator capabilities
- Easy replacement of the lexical implementation later
- Useful foundation for RAG and AI search scenarios

## 22. Limitations

This is a learning and architecture demonstration rather than a production-scale search engine.

The application-side keyword search retrieves the filtered document set and performs BM25-style ranking in application memory. For a large production corpus this can increase network transfer, memory, CPU, and latency.

A production implementation should evaluate native Cosmos full-text/hybrid capabilities available in the target deployment or a dedicated search service.

Candidate-window size and weights should be tuned using real relevance evaluations.

## 23. Final Architecture

```text
                Natural Language Query
                         │
             ┌───────────┴───────────┐
             │                       │
             ▼                       ▼
      Ollama Embedding         Keyword Tokenizer
             │                       │
             ▼                       ▼
       Cosmos Vector DB        BM25-style Ranking
       VectorDistance              │
       + DiskANN                   │
             │                     │
             ▼                     ▼
        Vector Rank            Keyword Rank
             │                     │
             └──────────┬──────────┘
                        ▼
                 Weighted RRF
                  Rank Constant 60
                        │
                        ▼
                  Final Results
                        │
                        ▼
             Ranking Diagnostics
             + Search Explanation
```

The core engineering principle is:

```text
Vector search:
    "What is this query conceptually about?"

Keyword search:
    "Which documents contain the important exact terms?"

RRF:
    "How should the two ranked lists be combined?"
```

For this local emulator project, that architecture is implemented using Cosmos DB vector search plus application-side lexical ranking and Weighted RRF.
