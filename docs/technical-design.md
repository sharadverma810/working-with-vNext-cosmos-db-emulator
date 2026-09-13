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

The RankConstant is deliberately large (60) to dampen the impact of high ranks and make the fusion stable across candidate windows. With this constant the algorithm prefers top-ranked documents from either list while still allowing mid-ranked candidates to contribute when both signals agree.

## 8. Implementation details

- Candidate windows: the vector query requests a larger candidate window (e.g. 100) to provide the keyword ranking and RRF with more documents to consider.
- Paging: the final API `top` parameter controls how many results are returned after fusion. Server-side continuation tokens are not required for small result windows but should be considered for large result sets.
- Tokenization: the keyword tokenizer preserves technical tokens (units, numeric specs) and lowercases others. Stopwords are removed for BM25 scoring.
- BM25-style ranking: the service computes a simplified BM25-like score using term frequency, inverse document frequency approximations from a local index/summary, and field weights (title, searchText, tags).

## 9. API contracts

- POST /api/hybrid/search
  - request: HybridSearchRequest { query, mode, vectorWeight, keywordWeight, filters, top }
  - response: HybridSearchResult { items[], explanation, diagnostics }

- POST /api/hybrid/seed?count={n}
  - seeds the HybridProducts container with representative embedded documents.

See the Implementation Walkthrough for concrete curl/Postman examples.

## 10. Diagnostics and observability

- The API returns lightweight explanation metadata per result (vectorRank, keywordRank, rrfScore) when the `explain=true` query flag is provided.
- Metrics: OpenTelemetry metrics include candidate counts, Ollama embedding latency, vector query latency, keyword ranking latency, RRF execution time, and result counts.
- Traces: distributed traces cover the end-to-end request, embedding RPC, Cosmos vector query, and the fusion step.

## 11. Operational considerations

- Ollama availability: the system tolerates Ollama transient failures by falling back to keyword-only mode if configured.
- Index maintenance: DiskANN indexes should be created or rebuilt during container initialization or maintenance windows.
- Cost and scaling: in production, vector candidate window size, Ollama throughput, and Cosmos RUs must be tuned for cost-performance balance.

## 12. References

- Implementation walkthrough: `docs/implementation-walkthrough.md`
- Emulator setup: `docs/Azure_Cosmos_DB_Emulator_vNext_Windows_Docker_Guide_Updated_PowerShell51.md`

---

Last updated: (local learning repo)
