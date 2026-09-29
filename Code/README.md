# RAG Project

A full-stack Retrieval-Augmented Generation (RAG) platform: upload documents (or add a URL), have them parsed, chunked, summarized, and embedded, then chat with an AI assistant that answers from that content with citations back to the source page.

The system is a three-service application:

| Service | Language / Framework | Responsibility |
|---|---|---|
| `backend` (**Api** + **Workers**) | .NET 10 / ASP.NET Core, Clean Architecture | REST API, auth, orchestration, RAG retrieval, chat, background job processing |
| `services/parsing-service` | Python 3.11+ / FastAPI / `unstructured` | Partitions & chunks raw files (PDF, DOCX, PPTX, TXT, MD, HTML) into structured content |
| `frontend` | Next.js 16 / React 19 / TypeScript | Web UI for managing projects, documents, and chats |

> The backend's code comments reference an earlier Python/FastAPI implementation (`server/routes/*.py`, `server/tasks.py`) — this repository is a from-scratch .NET rewrite of that original service, with the document-parsing step kept as a standalone Python microservice because it depends on the `unstructured` library.

## Table of contents

- [Architecture](#architecture)
- [How a document becomes searchable](#how-a-document-becomes-searchable)
- [How a chat message is answered](#how-a-chat-message-is-answered)
- [Retrieval strategies](#retrieval-strategies)
- [Tech stack](#tech-stack)
- [Repository layout](#repository-layout)
- [Data model](#data-model)
- [Getting started](#getting-started)
- [Configuration reference](#configuration-reference)
- [API surface](#api-surface)
- [Testing](#testing)

## Architecture

```
                         ┌─────────────────────┐
                         │   Frontend (Next.js) │
                         │  Clerk-authenticated  │
                         └──────────┬───────────┘
                                    │ REST (JWT bearer)
                                    ▼
┌──────────────────────────────────────────────────────────────────┐
│  Api  (ASP.NET Core, MediatR/CQRS)                                │
│   Controllers → Commands/Queries → Application services           │
└───────────┬───────────────────────────────┬────────────────────┬─┘
            │ enqueue job                    │ read/write          │ presigned upload
            ▼                                 ▼                     ▼
     ┌──────────────┐                 ┌───────────────┐     ┌─────────────────┐
     │  Hangfire      │                │  PostgreSQL   │     │  S3-compatible   │
     │  (Redis-backed)│                │  + pgvector   │     │  object storage  │
     └──────┬────────┘                 └───────────────┘     │  (Tigris)        │
            │ dequeue job                                     └─────────────────┘
            ▼
     ┌──────────────┐
     │  Workers      │  (same Application/Infrastructure code as the Api)
     │  host process │
     └──────┬────────┘
            │ ProcessDocumentCommand
            ▼
   ┌────────────────────┐     ┌────────────────────┐     ┌────────────────────┐
   │ Parsing Service     │     │ OpenAI (Semantic    │     │ Cohere Rerank       │
   │ (FastAPI/unstructured)│──▶│ Kernel: summarize,  │     │ (optional)          │
   │ partition + chunk    │     │ embed, answer, vary │     └────────────────────┘
   └────────────────────┘     └────────────────────┘
```

The `Api` and `Workers` executables both host the **same** `Application` + `Infrastructure` code (Clean Architecture): the Api receives HTTP requests and enqueues long-running work as Hangfire jobs; the Workers process dequeues and executes those jobs (currently document processing). Both talk to the same PostgreSQL database and Redis instance.

## How a document becomes searchable

Uploading a file or adding a URL kicks off `ProcessDocumentCommand`, run as a background job (`DocumentProcessingJob` → Hangfire → `Workers`). The document's `ProcessingStatus` is updated at each stage so the UI can poll progress:

1. **`partitioning`** — the raw file is fetched from S3 (or the URL is fetched via ScrapingBee for web sources) and streamed to the parsing service.
2. **`chunking`** — the parsing service (`services/parsing-service`) uses `unstructured` to partition the document (PDF/DOCX/PPTX/TXT/MD/HTML) and groups elements into chunks with `chunk_by_title`, separating out tables (as HTML) and images (as base64) from plain narrative text.
3. **`summarising`** — any chunk that contains a table or image is summarized by `SemanticKernelSummarizationService` (OpenAI `gpt-4.1` via Semantic Kernel) into embeddable text; plain-text chunks pass through unchanged.
4. **`vectorization`** — all processed chunks are embedded in a single batch (`OpenAiEmbeddingService`) and persisted as `DocumentChunk` rows with a `pgvector` embedding column, plus the original text/tables/images JSON (used later to build chat context).
5. **`completed`** — the document is now part of every chat search within its project.

## How a chat message is answered

`SendMessageCommand` (`POST /api/projects/{projectId}/chats/{chatId}/messages`):

1. Persists the user's message.
2. Loads the project's `ProjectSettings` (RAG strategy, model, thresholds, etc.) and the project's document IDs.
3. Calls `RagRetrievalService.RetrieveAsync`, which runs the configured retrieval strategy (see below), optionally reranks with Cohere, and returns the top chunks.
4. `RagContextBuilder` reconstructs each chunk's original text/tables/images and builds a citation (document filename + page number) for every chunk used.
5. `SemanticKernelChatAnswerService` (OpenAI `gpt-4o`) generates the answer from the question plus that context.
6. Persists the assistant's message with its citations and returns both messages to the client.

## Retrieval strategies

Configurable per project (`ProjectSettings.RagStrategy`), implemented in `RagRetrievalService`:

| Strategy | Behavior |
|---|---|
| `basic` (default/fallback) | Plain vector similarity search over `DocumentChunk` embeddings. |
| `hybrid` | Vector search **+** PostgreSQL full-text keyword search, combined with weighted **Reciprocal Rank Fusion** (`RrfFusion`, `VectorWeight` / `KeywordWeight`). |
| `multi-query-vector` | An LLM (`SemanticKernelQueryVariationService`) generates `NumberOfQueries` paraphrases of the question; each is vector-searched independently and the result sets are fused with RRF. |
| `multi-query-hybrid` | Same query-variation step, but each variation runs a full hybrid (vector + keyword) search before fusion. |

After retrieval, if `RerankingEnabled` is set, the fused results are reranked by `CohereRerankService` and truncated to `FinalContextSize` chunks before being sent to the LLM.

## Tech stack

**Backend**
- .NET 10 / ASP.NET Core Web API, API versioning (`Asp.Versioning`)
- Clean Architecture: `Domain` → `Application` (CQRS via **MediatR**, validation via **FluentValidation**) → `Infrastructure` → `Api` / `Workers`
- **PostgreSQL** + **pgvector** (via `Npgsql`, `Pgvector.EntityFrameworkCore`) as the primary store and vector index
- **Redis** + **Hangfire** for background job processing and dashboard (`/hangfire`)
- **Microsoft.Semantic Kernel** + **OpenAI** for summarization, embeddings, chat answers, and query-variation generation
- **Cohere Rerank API** for optional result reranking
- **ScrapingBee** for fetching URL-sourced documents
- **AWS S3 SDK** against an S3-compatible provider (Tigris by default) for document storage via presigned URLs
- **Clerk** JWT bearer authentication + webhook-driven user provisioning
- **Serilog** (structured console logging) and **OpenTelemetry** (tracing + metrics); traces are exported over OTLP to **Jaeger**, with log records attached to their spans as span events
- Health checks (`/health`) for Postgres, Redis, and the parsing service

**Parsing microservice**
- **FastAPI** + **Uvicorn**
- **`unstructured[all-docs]`** for partitioning PDF/DOCX/PPTX/TXT/MD/HTML and chunking by title

**Frontend**
- **Next.js 16** (App Router) + **React 19** + **TypeScript**
- **Tailwind CSS 4**
- **Clerk** (`@clerk/nextjs`) for auth
- `react-dropzone` for file uploads, `react-hot-toast` for notifications, `lucide-react` for icons

**Testing**
- xUnit + FluentAssertions across `Domain.Tests`, `Application.Tests`, and `Integration.Tests`
- `Integration.Tests` uses `Microsoft.AspNetCore.Mvc.Testing` (in-process `WebApplicationFactory`, see `Program.cs`'s exposed `partial class Program`) and `Testcontainers.PostgreSql` to run against a real, ephemeral Postgres instance

## Repository layout

```
.
├── docker-compose.yml            # api, worker, parsing-service, frontend, redis, postgres(pgvector)
├── .env.example                  # env vars consumed by docker-compose
├── backend/
│   ├── RagMigration.sln
│   ├── src/
│   │   ├── Domain/               # Entities (Project, ProjectDocument, ProjectSettings, Chat, Message, DocumentChunk, User), no external deps
│   │   ├── Application/          # CQRS commands/queries/handlers/validators, RAG services, interfaces (ports)
│   │   ├── Contracts/            # DTOs shared between Application and Api
│   │   ├── Infrastructure/       # EF Core, repositories, Semantic Kernel/OpenAI, Cohere, S3, ScrapingBee, Hangfire, auth
│   │   ├── Api/                  # ASP.NET Core host: controllers, middleware, Swagger, DI wiring
│   │   ├── SharedKernel/         # Base Entity<TId>
│   │   └── Workers/               # Background-job host (shares Application/Infrastructure with Api)
│   └── tests/
│       ├── Domain.Tests/
│       ├── Application.Tests/
│       └── Integration.Tests/
├── services/parsing-service/
│   ├── main.py                   # FastAPI app: POST /partition-and-chunk
│   ├── pyproject.toml
│   └── Dockerfile
└── frontend/
    ├── src/app/                  # Next.js App Router: (auth) sign-in/up, (dashboard) projects/chats
    ├── src/components/           # chat, projects, layout, ui
    └── src/lib/                  # api client, shared types
```

### Backend layers, in dependency order

`Domain` (no dependencies) ← `Application` (depends on `Domain`, `Contracts`) ← `Infrastructure` (depends on `Application`, `Domain`) ← `Api` / `Workers` (depend on `Application`, `Infrastructure`).

Every external dependency (database, OpenAI, Cohere, S3, ScrapingBee, the parsing service) is accessed through an interface defined in `Application/Common/Interfaces`, implemented in `Infrastructure`, and wired up in `Infrastructure/DependencyInjection.cs` — the Application layer never references a concrete provider.

## Data model

| Entity | Purpose |
|---|---|
| `User` | A Clerk-provisioned user (`ClerkId`), created via the `create-user` webhook. |
| `Project` | A workspace containing documents, chats, and one `ProjectSettings`. |
| `ProjectSettings` | Per-project RAG configuration: embedding model, `RagStrategy`, chunk/context sizes, similarity threshold, reranking, hybrid-search weights. |
| `ProjectDocument` | A file or URL added to a project, tracked through `ProcessingStatus` (`partitioning` → `chunking` → `summarising` → `vectorization` → `completed`). |
| `DocumentChunk` | A chunk of a processed document: embedded text, page number, type metadata, and the original text/tables/images JSON used to rebuild chat context. |
| `Chat` | A conversation scoped to a project. |
| `Message` | A user or assistant message in a chat, with assistant messages carrying serialized `CitationDto[]`. |

## Getting started

### Prerequisites

- .NET 10 SDK (backend)
- Node.js 18+ (frontend)
- Python 3.11–3.12 + [Poetry](https://python-poetry.org/) (parsing service, if running outside Docker)
- Docker + Docker Compose (recommended — runs everything, including Postgres/pgvector and Redis)
- Accounts/keys for: Clerk, OpenAI, ScrapingBee, Cohere, and an S3-compatible bucket (Tigris by default)

### Run everything with Docker Compose

```bash
cp .env.example .env
# fill in CLERK_AUTHORITY, NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY, CLERK_SECRET_KEY,
# OPENAI_API_KEY, SCRAPINGBEE_API_KEY, AWS_ACCESS_KEY_ID/AWS_SECRET_ACCESS_KEY, COHERE_API_KEY

docker compose up --build
```

This starts:

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| Api | http://localhost:8000 (Swagger UI in Development, `/health`, `/hangfire`) |
| Parsing service | http://localhost:8001/docs |
| Postgres (pgvector) | localhost:5435 |
| Redis | localhost:6380 |
| Jaeger UI (traces + attached logs) | http://localhost:16686 (OTLP on 4317 gRPC / 4318 HTTP) |

The Api applies EF Core migrations automatically on startup (`Database.Migrate()`).

### Running services individually

**Backend (Api)**
```bash
cd backend
dotnet run --project src/Api
```

**Backend (Workers)**
```bash
cd backend
dotnet run --project src/Workers
```

Both need `ConnectionStrings:Postgres` and `ConnectionStrings:Redis` (see `appsettings.json` / `appsettings.Development.json`) and the same provider keys as Docker Compose, either via `dotnet user-secrets`, environment variables, or `appsettings.Development.json`.

To see traces when running outside Compose, start just Jaeger (`docker compose up jaeger`); the Api and Workers export to `http://localhost:4317` by default.

**Parsing service**
```bash
cd services/parsing-service
poetry install
poetry run uvicorn main:app --reload --port 8001
```

**Frontend**
```bash
cd frontend
npm install
npm run dev
```

Set `NEXT_PUBLIC_API_URL` (defaults to `http://localhost:8000` when built via Docker) and `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` / `CLERK_SECRET_KEY` for the frontend.

## Configuration reference

Read from `IConfiguration` (environment variables use the `Section__Key` convention):

| Key | Used by | Purpose |
|---|---|---|
| `ConnectionStrings:Postgres` | Api, Workers | PostgreSQL connection string |
| `ConnectionStrings:Redis` | Api, Workers | Redis connection string (Hangfire storage) |
| `Clerk:Authority` | Api | Clerk JWT issuer for bearer-token validation |
| `S3:BucketName` | Infrastructure | Object storage bucket for uploaded documents |
| `AWS:ServiceURL`, `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, `AWS_REGION` | Infrastructure | S3-compatible storage credentials/endpoint (Tigris by default) |
| `ParsingService:BaseUrl` | Api, Workers | Base URL of the Python parsing microservice |
| `ScrapingBee:ApiKey` | Infrastructure | Crawling URL-sourced documents |
| `OpenAI:ApiKey` | Infrastructure | Summarization (`gpt-4.1`), chat answers (`gpt-4o`), embeddings, query variation |
| `Cohere:ApiKey` | Infrastructure | Reranking (only called when a project has `RerankingEnabled=true`) |
| `Cors:AllowedOrigins` | Api | Allowed origins for the frontend |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Api, Workers | OTLP (gRPC) endpoint for traces; defaults to `http://localhost:4317`, set to `http://jaeger:4317` in Docker Compose |

Frontend (`frontend/.env*`): `NEXT_PUBLIC_API_URL`, `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY`, `CLERK_SECRET_KEY`.

## API surface

All routes are versioned (`/v1/api/...`) with an unversioned alias (`/api/...`) and require a Clerk bearer token except the webhook endpoint.

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/create-user` | Clerk webhook — provisions a `User` on `user.created` |
| `GET` | `/api/projects` | List the current user's projects |
| `POST` | `/api/projects` | Create a project |
| `GET` | `/api/projects/{projectId}` | Get a project |
| `DELETE` | `/api/projects/{projectId}` | Delete a project |
| `GET` / `PUT` | `/api/projects/{projectId}/settings` | Get/update RAG settings |
| `GET` | `/api/projects/{projectId}/chats` | List chats in a project |
| `POST` | `/api/projects/{projectId}/chats/{chatId}/messages` | Send a message, get the AI response + citations |
| `POST` | `/api/chats` | Create a chat |
| `GET` | `/api/chats/{chatId}` | Get a chat with its messages |
| `DELETE` | `/api/chats/{chatId}` | Delete a chat |
| `GET` | `/api/projects/{projectId}/files` | List a project's documents |
| `GET` | `/api/projects/{projectId}/files/{fileId}/chunks` | Inspect a document's chunks |
| `POST` | `/api/projects/{projectId}/files/upload-url` | Get a presigned S3 upload URL |
| `POST` | `/api/projects/{projectId}/files/confirm-upload` | Confirm an upload and enqueue processing |
| `POST` | `/api/projects/{projectId}/urls` | Add a URL document and enqueue processing |
| `DELETE` | `/api/projects/{projectId}/files/{fileId}` | Delete a document |

Full request/response schemas are available via Swagger UI at the Api root when running in Development.

## Testing

```bash
cd backend
dotnet test
```

- `Domain.Tests` — entity/invariant unit tests
- `Application.Tests` — command/query handler unit tests
- `Integration.Tests` — full HTTP pipeline tests via `WebApplicationFactory`, backed by a real Postgres container (`Testcontainers.PostgreSql`) — requires Docker to be running
