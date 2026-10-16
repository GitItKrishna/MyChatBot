# Roadmap

Each step is one GitHub issue, one feature branch (`feature/NN-name`), one pull request, and one document in `docs/`.

| # | Step | Deliverable | Topics |
|---|------|-------------|--------|
| 01 | Solution, Aspire, FastEndpoints skeleton | Solution, AppHost, ServiceDefaults, API with `/api/status` | Aspire, FastEndpoints, project layout |
| 02 | Blazor chat UI | Chat page calling the API | Blazor Server, service discovery |
| 03 | PostgreSQL + pgvector | Postgres container via Aspire, EF Core entities, migration applied at startup | `vector` column type, HNSW index |
| 04 | PDF extraction and chunking | Page-aware text extraction and chunking | Chunk size, overlap |
| 05 | Embeddings (Titan on Bedrock) | Text to vector | Embeddings, cosine similarity |
| 06 | `/ingest` endpoint | S3 PDF -> extract -> chunk -> embed -> store | Ingestion pipeline |
| 07 | `/query` retrieval | Question -> embed -> nearest chunks | Vector search |
| 08 | Answer generation (Nova 2 Lite) | Prompt with context; answer with sources and page numbers | Grounding, prompt design, citations |
| 09 | Retrieval quality | top-k, score thresholds, overlap tuning, hybrid search, eval set | Retrieval evaluation |
| 10 | Containerize + 3-tier VPC | Dockerfile; CDK VPC with public, private, isolated subnets | Network tiers |
| 11 | RDS PostgreSQL + Secrets Manager | Isolated database, password in Secrets Manager | Least privilege |
| 12 | ECS Fargate + public ALB | API in private subnets behind an ALB | Traffic flow |
| 13 | ECR + VPC endpoints | Private image pulls and AWS API access without NAT | NAT Gateway cost |

## Decisions

- **IaC:** AWS CDK in C#. Terraform was rejected to avoid managing state files.
- **Embeddings:** Amazon Titan Text Embeddings V2, 1024 dimensions.
- **Generation:** Amazon Nova 2 Lite via Bedrock. The exact model ID is verified in step 08.
- **Data access:** EF Core with Npgsql and `Pgvector.EntityFrameworkCore`.
- **Local development:** Aspire runs Postgres in Docker; steps 01-09 need no AWS resources except Bedrock calls.
- **Cost:** steps 10-13 create billable resources (NAT, ALB, RDS, Fargate). Run `cdk destroy` when idle.
