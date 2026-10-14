# MyChatBot: RAG Chatbot on AWS (.NET, Blazor, Aspire, FastEndpoints)

Drop PDFs into S3, ingest them, and ask questions about your own documents. Answers are grounded in your documents and cite sources with page numbers.

```
PDFs in S3 -> text extraction -> chunking -> Titan embeddings (Bedrock)
          -> PostgreSQL + pgvector (RDS) -> Nova 2 Lite answers with sources
```

## Tech stack

| Layer | Tech |
|-------|------|
| UI | Blazor |
| API | .NET + FastEndpoints (`/ingest`, `/query`) |
| Orchestration (local) | .NET Aspire |
| Embeddings / LLM | Amazon Titan / Amazon Nova 2 Lite via Bedrock |
| Vector store | PostgreSQL + pgvector (RDS in AWS) |
| Infra | AWS CDK (C#): 3-tier VPC, ALB, ECS Fargate, RDS, Secrets Manager |

## Target AWS architecture

- Public **ALB** in public subnets
- API on **ECS Fargate** in private subnets
- **RDS** in isolated subnets (no internet route)
- DB password in **Secrets Manager**
- Later: **ECR + VPC endpoints** instead of NAT Gateways to cut cost

## Learning path

Start with [docs/01-how-rag-works.md](docs/01-how-rag-works.md), then follow [docs/00-roadmap.md](docs/00-roadmap.md). Each step adds one feature through one Pull Request and one lesson document in `docs/`.

## Progress

| Step | Status |
|------|--------|
| 00 Project docs and roadmap | Done |
| 01 Solution + Aspire + FastEndpoints skeleton | Next |
| 02-13 | See roadmap |

## Prerequisites

.NET 10 SDK, Docker, AWS CLI (configured with a profile that has Bedrock access), `gh` CLI.
