# Cosmos Learning — Azure Cosmos DB vNext Emulator Work

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Azure Cosmos DB](https://img.shields.io/badge/Azure%20Cosmos%20DB-vNext-0078D4?logo=microsoftazure&logoColor=white)](https://azure.microsoft.com/products/cosmos-db/)
[![Ollama](https://img.shields.io/badge/Ollama-Local%20AI-black?logo=ollama&logoColor=white)](https://ollama.com/)
[![License](https://img.shields.io/badge/license-see%20LICENSE-blue.svg)](LICENSE)

A hands-on engineering playground for learning and demonstrating **Azure Cosmos DB vNext capabilities locally**, using **.NET, the Cosmos DB Linux Docker Emulator, Ollama, and practical API examples**.

The repository is designed to be more than a collection of code samples. Each capability is implemented as a small, independently understandable learning module with working APIs, sample requests, technical design notes, implementation walkthroughs, and tests where appropriate.

> **Learning by building:** start with a capability, run it locally, inspect the implementation, experiment with the sample requests, and then extend it.

---

## Table of Contents

- [Why This Repository](#why-this-repository)
- [What You Will Learn](#what-you-will-learn)
- [Current Capabilities](#current-capabilities)
- [Architecture](#architecture)
- [Repository Structure](#repository-structure)
- [Getting Started](#getting-started)
- [Local Services](#local-services)
- [Learning Path](#learning-path)
- [Hybrid Search](#hybrid-search)
- [Documentation](#documentation)
- [Sample Requests](#sample-requests)
- [Testing](#testing)
- [Development Workflow](#development-workflow)
- [Troubleshooting](#troubleshooting)
- [Design Principles](#design-principles)
- [Contributing](#contributing)
- [Issues and Feature Requests](#issues-and-feature-requests)
- [Security](#security)
- [License](#license)
- [Acknowledgements](#acknowledgements)

---

## Why This Repository

Azure Cosmos DB is evolving beyond traditional document CRUD scenarios into a platform for modern application workloads involving:

- AI-assisted applications
- semantic search
- vector search
- hybrid retrieval
- RAG
- agent memory
- application resilience
- observability
- performance optimization
- advanced query patterns
- modern .NET API architectures

These capabilities can be difficult to learn from isolated documentation examples.

This repository aims to provide a **repeatable local laboratory** where developers can:

1. Start the required local services.
2. Run working .NET APIs.
3. Seed realistic data.
4. Execute requests with Postman or `curl`.
5. Inspect Cosmos DB documents and indexes.
6. Understand the implementation behind each capability.
7. Modify the code and experiment.
8. Compare alternative approaches.
9. Contribute additional examples.

---

## What You Will Learn

The repository focuses on practical engineering rather than only API syntax.

You will learn how to work with:

### Azure Cosmos DB

- databases and containers
- partition keys
- CRUD operations
- queries
- indexing
- vector embeddings
- vector indexes
- DiskANN
- vector similarity search
- filtering
- emulator-based local development

### AI and Search

- local embeddings with Ollama
- embedding generation
- semantic search
- keyword/lexical search
- BM25-style ranking
- hybrid retrieval
- Reciprocal Rank Fusion (RRF)
- weighted RRF
- ranking diagnostics
- search relevance experimentation

### .NET

- ASP.NET Core Web API
- feature-oriented organization
- dependency injection
- repositories
- service-layer orchestration
- cancellation tokens
- request/response models
- error handling
- resilience
- observability
- testable application components

The roadmap will continue to expand as additional Cosmos DB capabilities are added.

---

## Current Capabilities

The repository currently contains working demonstrations around:

| Capability | Status |
|---|---|
| Local Cosmos DB vNext Emulator | ✅ |
| .NET API integration | ✅ |
| Cosmos DB document operations | ✅ |
| Vector search | ✅ |
| Ollama local embeddings | ✅ |
| DiskANN vector index | ✅ |
| Metadata filtering | ✅ |
| Large search-oriented product catalog | ✅ |
| Keyword / BM25-style ranking | ✅ |
| Vector-only search mode | ✅ |
| Keyword-only search mode | ✅ |
| Hybrid search | ✅ |
| Weighted RRF | ✅ |
| Ranking diagnostics | ✅ |
| Search explanation metadata | ✅ |
| Error handling and resilience work | 🚧 |
| Additional advanced Cosmos DB capabilities | 🚧 |

The status table is intentionally updated as the learning roadmap progresses.

---

## Architecture

At a high level, the project follows a feature-oriented .NET architecture.

```text
                         ┌─────────────────────┐
                         │       Client        │
                         │ Postman / curl / UI │
                         └──────────┬──────────┘
                                    │
                                    ▼
                         ┌─────────────────────┐
                         │ ASP.NET Core API    │
                         │                     │
                         │ Feature-oriented    │
                         │ endpoints           │
                         └──────────┬──────────┘
                                    │
                    ┌───────────────┼────────────────┐
                    │               │                │
                    ▼               ▼                ▼
               Application       Cosmos DB        Local AI
                 Services       Repository         Ollama
                    │               │                │
                    │               ▼                │
                    │       Cosmos DB vNext          │
                    │          Emulator              │
                    │               │                │
                    └───────────────┴────────────────┘
                                    │
                                    ▼
                              API Response
```

The repository intentionally keeps infrastructure concerns separate from feature/application logic so that individual demonstrations remain easy to understand.

---

# Hybrid Search

One of the completed demonstrations combines semantic and lexical retrieval.

```text
                         User Query
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
       Ollama Embedding                 Keyword Tokenizer
              │                               │
              ▼                               ▼
      Cosmos Vector Search             BM25-style Ranking
      VectorDistance + DiskANN               │
              │                               │
              ▼                               ▼
         Vector Rank                     Keyword Rank
              │                               │
              └───────────────┬───────────────┘
                              ▼
                     Weighted RRF Fusion
                       Rank Constant 60
                              │
                              ▼
                       Final Ranking
                              │
                              ▼
                   Diagnostics + Explanation
```

The hybrid implementation supports:

- vector-only search
- keyword-only search
- hybrid search
- vector weighting
- keyword weighting
- category filtering
- minimum/maximum price filtering
- ranking diagnostics
- search explanation

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

The API exposes information such as:

```json
{
  "vectorRank": 30,
  "keywordRank": 2,
  "vectorContribution": 0.011111111111111112,
  "keywordContribution": 0.016129032258064516,
  "rrfScore": 0.02724014336917563
}
```

This makes the ranking behavior inspectable instead of treating the search engine as a black box.

### Important emulator note

The local Cosmos DB vNext Emulator used during development rejected the native `FullTextScore()` query function.

Therefore the current local hybrid implementation intentionally uses:

```text
Cosmos DB:
    VectorDistance + DiskANN

Application:
    BM25-style keyword ranking
    Weighted RRF
```

This repository does **not** claim that the local emulator is executing native Cosmos full-text hybrid ranking.

The distinction between emulator-compatible application-side hybrid search and native Cosmos capabilities available in other deployment environments is documented in detail in the technical design.

---

## Repository Structure

```text
.
├── .github/
│   └── prompt/
│
├── docs/
│   ├── technical-desing.md
│   └── implementation-walkthrough.md
│
├── scripts/
│   └── sample-requests/
│
├── src/
│   └── ...
│
├── tests/
│   └── CosmosLearning.Tests/
│
├── .gitignore
├── CosmosLearning.slnx
├── LICENSE
└── README.md
```

### `src/`

Application source code.

Feature implementations and infrastructure live here.

### `tests/`

Automated tests and test-related code.

### `scripts/sample-requests/`

Reusable HTTP examples for learning and manual verification.

### `docs/`

Technical and implementation documentation.

### `README.md`

The entry point for developers discovering the repository.

---

# Getting Started

## Prerequisites

You should have:

- Windows with Docker Desktop
- .NET 10 SDK
- Docker
- Ollama
- Git
- Postman or `curl.exe`

Verify your tools:

```powershell
dotnet --version
docker --version
ollama --version
```

---

## 1. Clone the Repository

```powershell
git clone <YOUR-REPOSITORY-URL>
cd <YOUR-REPOSITORY-DIRECTORY>
```

Replace the placeholder with the repository URL after publishing it.

---

## 2. Start Ollama

Install the embedding model:

```powershell
ollama pull mxbai-embed-large
```

Verify:

```powershell
ollama list
```

The application expects Ollama locally at:

```text
http://localhost:11434
```

---

## 3. Start the Cosmos DB vNext Emulator

Start the Cosmos DB vNext Linux Docker Emulator using the project's existing Docker configuration.

The API expects:

```text
https://localhost:8081/
```

Make sure the emulator is running before starting the API.

---

## 4. Restore Dependencies

From the API project directory:

```powershell
dotnet restore
```

---

## 5. Build

```powershell
dotnet build
```

A successful build confirms that the project and its dependencies are available locally.

---

## 6. Run the API

```powershell
dotnet run
```

The local API used by the current implementation runs on:

```text
http://localhost:5142
```

---

## 7. Initialize and Seed Data

For the hybrid-search demonstration:

```powershell
curl.exe -X POST "http://localhost:5142/api/hybrid/seed?count=1000"
```

The application generates the search-oriented catalog, creates Ollama embeddings, and stores the products in:

```text
VectorDemoDb
    └── HybridProducts
```

---

# Local Services

The current local development environment is:

| Service | Address | Purpose |
|---|---|---|
| ASP.NET Core API | `http://localhost:5142` | Application/API |
| Cosmos DB vNext Emulator | `https://localhost:8081/` | Local Cosmos DB |
| Ollama | `http://localhost:11434` | Local embeddings |

The exact API port can be changed through the project's launch/configuration settings if required.

---

# Learning Path

The repository is intended to be explored incrementally.

A recommended learning sequence is:

```text
1. Local environment
       ↓
2. Cosmos DB fundamentals
       ↓
3. .NET API integration
       ↓
4. Vector search
       ↓
5. Ollama embeddings
       ↓
6. Large search-oriented dataset
       ↓
7. Keyword search
       ↓
8. Hybrid search
       ↓
9. Weighted RRF
       ↓
10. Ranking diagnostics
       ↓
11. Error handling & resilience
       ↓
12. Observability
       ↓
13. Performance
       ↓
14. Advanced Cosmos DB capabilities
       ↓
15. AI / RAG / Agent scenarios
```

The exact roadmap will evolve as the repository grows.

---

# Documentation

Detailed documentation is maintained under `docs/`.

## Technical Design

[`docs/technical-desing.md`](docs/technical-desing.md)

Covers:

- architecture
- component responsibilities
- Cosmos DB container design
- vector configuration
- DiskANN
- Ollama embeddings
- keyword ranking
- BM25-style scoring
- RRF
- weighted RRF
- candidate windows
- ranking diagnostics
- emulator limitations
- production considerations

## Implementation Walkthrough

[`docs/implementation-walkthrough.md`](docs/implementation-walkthrough.md)

Provides a step-by-step guide covering:

- prerequisites
- local setup
- Ollama
- Cosmos DB emulator
- configuration
- build
- run
- initialization
- data seeding
- search verification
- weighted RRF experiments
- filtering
- troubleshooting
- validation checklist

> If you are new to this project, start with the implementation walkthrough and then read the technical design.

---

# Sample Requests

Reusable examples are maintained under:

```text
scripts/sample-requests/
```

The examples are intended to be executed with Postman, PowerShell, or `curl.exe`.

Typical experiments include:

### Semantic search

```text
a keyboard for someone who types code all day
```

### Exact technical search

```text
8000Hz 0.5ms anti-ghosting
```

### Office intent

```text
quiet keyboard for an office shared workspace
```

### Vector-heavy hybrid

```json
{
  "vectorWeight": 2,
  "keywordWeight": 1
}
```

### Keyword-heavy hybrid

```json
{
  "vectorWeight": 1,
  "keywordWeight": 2
}
```

These experiments are intentionally different so that developers can observe how the retrieval strategy affects the result ranking.

---

# Testing

Run the complete test suite with:

```powershell
dotnet test
```

For a specific test project:

```powershell
dotnet test tests/CosmosLearning.Tests
```

Tests should be added alongside new capabilities where practical.

For search features, useful tests include:

- request validation
- tokenizer behavior
- BM25 scoring
- RRF calculations
- weight handling
- filtering
- ranking order
- edge cases
- repository behavior

---

# Development Workflow

A recommended workflow for contributors is:

```text
Create/choose a capability
        ↓
Write a small design note
        ↓
Implement feature
        ↓
Add/update tests
        ↓
Add sample request
        ↓
Run locally
        ↓
Verify with emulator
        ↓
Document behavior/limitations
        ↓
Commit
        ↓
Open Pull Request
```

Keep commits focused.

Good examples:

```text
Add hybrid search API
Add weighted RRF ranking
Add hybrid search walkthrough
Add keyword tokenizer tests
```

Avoid combining unrelated changes in a single commit.

---

# Troubleshooting

## Cosmos DB connection problems

Verify the emulator is running:

```text
https://localhost:8081/
```

Then verify the application is configured for the same endpoint.

---

## Ollama problems

Check:

```powershell
ollama list
```

Ensure:

```text
mxbai-embed-large
```

is installed.

---

## `Document does not contain an id field`

The Cosmos SDK used by this project uses Newtonsoft.Json serialization.

Cosmos document models should use the appropriate Newtonsoft attributes, for example:

```csharp
[JsonProperty("id")]
public string Id { get; set; }
```

---

## `FullTextScore` is not recognized

If the local emulator returns:

```text
SC2005: 'FullTextScore' is not a recognized built-in function name.
```

do not assume the application-side hybrid implementation is broken.

The current local implementation deliberately avoids depending on that unsupported query function:

```text
Vector:
    Cosmos VectorDistance + DiskANN

Keyword:
    Application-side BM25-style ranking

Fusion:
    Application-side Weighted RRF
```

See [`docs/technical-desing.md`](docs/technical-desing.md) for the design rationale.

---

# Design Principles

This repository follows several principles.

## 1. Learn by running

Every major capability should have a way to execute and observe it locally.

## 2. Explain the why, not only the how

Documentation should explain architectural decisions and trade-offs.

## 3. Keep examples realistic

The search catalog intentionally contains differentiated products and meaningful technical terms instead of completely generic placeholder data.

## 4. Make behavior observable

Where useful, APIs expose diagnostic information so developers can understand the system.

## 5. Separate concerns

Controllers, application services, repositories, search algorithms, and infrastructure should have clear responsibilities.

## 6. Document limitations honestly

A local emulator limitation should be clearly identified rather than hidden behind a misleading implementation.

## 7. Prefer incremental complexity

Each capability should build on concepts introduced earlier.

---

# Contributing

Contributions are welcome.

You can contribute by:

- fixing bugs
- improving documentation
- adding tests
- adding sample requests
- improving examples
- proposing architecture improvements
- implementing additional Cosmos DB capabilities
- improving developer experience
- adding performance experiments
- adding AI/RAG demonstrations

## Before contributing

Please:

1. Check existing issues and pull requests.
2. Avoid duplicating an existing effort.
3. Keep the change focused.
4. Add tests where appropriate.
5. Update documentation for user-visible behavior.
6. Verify the project builds locally.
7. Verify relevant functionality against the local emulator.
8. Explain important architectural decisions in the pull request.

## Pull Request checklist

A good pull request should answer:

- What problem does this solve?
- What changed?
- Why was this approach selected?
- How can someone reproduce it?
- What tests were added or updated?
- Are there emulator limitations?
- Does documentation need updating?

---

# Issues and Feature Requests

Please use GitHub Issues for:

- bug reports
- feature requests
- documentation improvements
- questions about the examples
- proposed learning topics

When reporting a problem, include:

- operating system
- .NET version
- Docker version
- emulator version/configuration
- Ollama version/model where relevant
- exact request/command
- relevant error message
- steps to reproduce

Please avoid posting secrets, credentials, connection strings containing sensitive values, or private data.

---

# Security

This repository is designed for local learning and experimentation.

Do not commit:

- passwords
- API keys
- tokens
- certificates/private keys
- production connection strings
- personal data
- proprietary data

Use local development configuration and environment-specific secret management where appropriate.

If you discover a security vulnerability in the repository, please avoid publicly disclosing exploit details in a normal issue until the maintainers have had an opportunity to assess it.

---

# License

See [`LICENSE`](LICENSE) for the applicable license.

---

# Acknowledgements

This project is intended as a practical learning companion for technologies including:

- Microsoft Azure Cosmos DB
- Azure Cosmos DB vNext Emulator
- ASP.NET Core
- .NET
- Ollama
- Docker

Please refer to the official documentation of each technology for authoritative product behavior and production guidance.

---

# Project Status

This repository is an actively evolving learning project.

Some capabilities are complete, while others are intentionally introduced incrementally.

The goal is not simply to produce a finished application. The goal is to provide a **public, reproducible engineering laboratory** where developers can understand modern Cosmos DB capabilities by building and experimenting with them.

---

## ⭐ If This Repository Helps You

If you find the examples useful:

- ⭐ Star the repository
- 👀 Follow the project
- 🐛 Report issues
- 💡 Suggest learning topics
- 🔧 Submit improvements
- 📖 Improve documentation
- 🔀 Open a pull request

The best contribution is often a small improvement that makes the next developer's learning experience easier.

---

## Quick Start

For experienced developers who already have the prerequisites installed:

```powershell
# Clone
git clone <YOUR-REPOSITORY-URL>

# Enter repository
cd <YOUR-REPOSITORY-DIRECTORY>

# Restore
dotnet restore

# Build
dotnet build

# Start Ollama model
ollama pull mxbai-embed-large

# Start the Cosmos DB vNext Emulator separately

# Run API
dotnet run

# Seed hybrid catalog
curl.exe -X POST "http://localhost:5142/api/hybrid/seed?count=1000"

# Search
curl.exe -X POST "http://localhost:5142/api/hybrid/search" `
  -H "Content-Type: application/json" `
  -d '{"query":"8000Hz 0.5ms anti-ghosting","mode":"hybrid","vectorWeight":1,"keywordWeight":1,"maximumPrice":20000,"top":5}'
```

For the full setup, see:

**[`docs/implementation-walkthrough.md`](docs/implementation-walkthrough.md)**

For the architecture and technical decisions, see:

**[`docs/technical-desing.md`](docs/technical-desing.md)**
