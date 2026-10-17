# MyChatBot: RAG Chatbot on AWS (.NET, Blazor, Aspire, FastEndpoints)

[![CI](https://github.com/GitItKrishna/MyChatBot/actions/workflows/ci.yml/badge.svg)](https://github.com/GitItKrishna/MyChatBot/actions/workflows/ci.yml)
[![CodeQL](https://github.com/GitItKrishna/MyChatBot/actions/workflows/codeql.yml/badge.svg)](https://github.com/GitItKrishna/MyChatBot/actions/workflows/codeql.yml)

Ingest PDFs from S3 and answer questions about them. Answers are grounded in the retrieved passages and cite file names and page numbers.

```
PDFs in S3 -> text extraction -> chunking -> Titan embeddings (Bedrock)
          -> PostgreSQL + pgvector (RDS) -> Nova 2 Lite answers with sources
```

## Tech stack

| Layer | Tech |
|-------|------|
| UI | Blazor Server |
| API | .NET + FastEndpoints (`/ingest`, `/query`) |
| Local orchestration | .NET Aspire |
| Data access | Entity Framework Core + Npgsql + pgvector |
| PDF extraction | PdfPig |
| Embeddings / LLM | Amazon Titan / Amazon Nova 2 Lite via Bedrock |
| Vector store | PostgreSQL + pgvector (RDS in AWS) |
| Infrastructure | AWS CDK (C#): 3-tier VPC, ALB, ECS Fargate, RDS, Secrets Manager |

## Target AWS architecture

- Public ALB in public subnets
- API on ECS Fargate in private subnets
- RDS in isolated subnets (no internet route)
- Database password in Secrets Manager
- ECR + VPC endpoints instead of NAT Gateways to reduce cost

## Documentation

- [docs/00-roadmap.md](docs/00-roadmap.md): step list and decisions
- [docs/01-how-rag-works.md](docs/01-how-rag-works.md): RAG pipeline overview
- [docs/ci-and-security.md](docs/ci-and-security.md): CI, coverage, CodeQL, dependency audit
- One document per implemented step in `docs/`

Each step is delivered as one issue, one feature branch and one pull request.

## Progress

| Step | Status |
|------|--------|
| 01 Solution, Aspire, FastEndpoints skeleton | Done ([doc](docs/02-step-01-skeleton.md)) |
| 02 Blazor chat UI | Done ([doc](docs/03-step-02-blazor-ui.md)) |
| 03 PostgreSQL + pgvector with EF Core | Done ([doc](docs/04-step-03-postgres-pgvector.md)) |
| 04 PDF extraction and chunking | Done ([doc](docs/05-step-04-pdf-chunking.md)) |
| 05 Embeddings (Titan on Bedrock) | Next |
| 06-13 | See roadmap |

## Run

```bash
dotnet run --project src/MyChatBot.AppHost
```

## Test

```bash
dotnet test
```

Try the chunker on the sample document (`POST /api/chunk-preview`, see [step 04](docs/05-step-04-pdf-chunking.md)).

## Prerequisites

.NET 10 SDK, Docker (running; Aspire starts Postgres in a container), AWS CLI with a profile that has Bedrock access, `gh` CLI.
