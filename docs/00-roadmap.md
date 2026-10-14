# Roadmap: step by step, one feature = one branch = one Pull Request

Every step below follows the same loop (this is also how you earn the GitHub badges, see `docs/github-achievements.md`):

1. Open a **GitHub Issue** for the step.
2. Create a **feature branch** (`feature/NN-name`).
3. Build the feature and write `docs/NN-name.md` (the lesson) and update the README.
4. Open a **Pull Request**, merge it, and the issue closes.

| # | Step | What you build | What you learn |
|---|------|----------------|----------------|
| 01 | Solution + Aspire + FastEndpoints skeleton | .NET solution, Aspire AppHost, ServiceDefaults, API with `/health` | What Aspire is, what FastEndpoints is, project layout |
| 02 | Blazor chat UI shell | A chat page that calls the API | Blazor basics, service discovery in Aspire |
| 03 | PostgreSQL + pgvector locally | Postgres container via Aspire, schema bootstrap on startup | What a vector database is, `vector` column type |
| 04 | PDF text extraction + chunking | Read a PDF page by page and split into chunks | Why chunking exists, chunk size and overlap |
| 05 | Embeddings (Amazon Titan on Bedrock) | Turn text into vectors | What an embedding is, cosine similarity |
| 06 | `/ingest` endpoint | S3 PDF -> extract -> chunk -> embed -> store | The "offline" half of RAG |
| 07 | `/query` retrieval | Question -> embed -> nearest chunks | The "online" half of RAG |
| 08 | Answer generation (Amazon Nova 2 Lite) | Prompt with context, answer with sources and page numbers | Grounding, prompt design, citations |
| 09 | Retrieval quality | top-k, score thresholds, overlap tuning, hybrid search, a small eval set | Why retrieval matters as much as the LLM |
| 10 | Containerize + 3-tier VPC (IaC) | Dockerfile, AWS CDK (C#) for VPC with public, private, isolated subnets | Why each network tier exists |
| 11 | RDS PostgreSQL + Secrets Manager | Isolated database, password in Secrets Manager | Least privilege, no internet route |
| 12 | ECS Fargate + public ALB | API in private subnets behind a load balancer | How traffic flows end to end |
| 13 | Cut NAT cost: ECR + VPC endpoints | Private image pulls and AWS API access without NAT | Why NAT Gateways are expensive |

## Decisions already made (change them if you disagree)

- **IaC tool: AWS CDK in C# (confirmed).** Same language as the app, so you learn one syntax. Terraform was considered and rejected to avoid managing state files.
- **Embeddings:** Amazon Titan Text Embeddings V2 (1024 dimensions).
- **Generation:** Amazon Nova 2 Lite via Bedrock (exact model ID is verified in step 08, since IDs and regional availability change).
- **Local dev:** Aspire runs Postgres in Docker so you never need AWS to learn steps 01-09.
- **AWS cost:** steps 01-09 cost almost nothing (only Bedrock calls, pennies). Steps 10-13 create billable resources (NAT, ALB, RDS, Fargate). Always run `cdk destroy` when done for the day.
