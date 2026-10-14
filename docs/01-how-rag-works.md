# How RAG works, explained for a complete beginner

## The problem

An LLM (like Nova or Claude) only knows what it saw in training. It has never read **your** PDFs. If you ask "What is our refund policy?", it will either say it doesn't know or, worse, **make something up**.

You cannot paste 500 pages into every question (too long, too slow, too expensive). So we do this instead:

> **RAG = Retrieval-Augmented Generation.** First *retrieve* the few relevant passages from your documents, then ask the LLM to *generate* an answer using only those passages.

Think of an open-book exam. The LLM is the student, your PDFs are the textbook, and retrieval is the index at the back of the book that finds the right pages.

## The two halves

### Half 1: Ingestion (done once per document, "offline")

```
PDF in S3 -> extract text per page -> split into chunks -> embed each chunk -> store in PostgreSQL
```

1. **Extract text.** Read the PDF and keep track of which page each piece of text came from (that is how we show page numbers later).
2. **Chunk.** Split the text into pieces of roughly 500-1000 characters. Why? A whole PDF is too big to match precisely, and a single sentence loses context. Chunks are the sweet spot. Neighboring chunks **overlap** a little so an idea cut at the boundary is not lost.
3. **Embed.** Send each chunk to Amazon Titan. It returns a list of ~1024 numbers (a **vector**). Texts with similar *meaning* get vectors that point in similar directions, even if they share no words. "car" and "automobile" end up close together.
4. **Store.** Save chunk text, page number, source file and vector in PostgreSQL. The **pgvector** extension adds a `vector` column type and fast "find nearest vectors" search.

### Half 2: Query (every time someone asks a question, "online")

```
Question -> embed it -> find nearest chunks -> build prompt -> LLM answers -> return answer + sources
```

1. **Embed the question** with the same Titan model, producing a vector.
2. **Similarity search.** Ask Postgres for the top-k (e.g. 5) chunks whose vectors are closest to the question vector (cosine distance).
3. **Build a prompt:** "Answer using ONLY the context below. If the answer isn't there, say you don't know. Context: [chunk 1] [chunk 2] ... Question: ..."
4. **Generate.** Nova 2 Lite reads that prompt and writes the answer.
5. **Return** the answer plus the sources (file name, page number) of the chunks used. That is the "grounded" part: you can check every claim.

## A tiny worked example

Your PDF says (page 7): *"Customers may return items within 30 days for a full refund."*

- Ingestion stores that chunk with page = 7 and its vector.
- You ask: *"Can I get my money back after three weeks?"*
- No word matches ("money back" vs "refund"), but the vectors are close, so the chunk is retrieved.
- The LLM answers: "Yes, returns within 30 days get a full refund. (policy.pdf, page 7)".

## Why retrieval matters as much as the LLM

The LLM can only answer from what retrieval hands it. If the right chunk is not in the top-k, even the best model fails or guesses. Most RAG quality problems are retrieval problems: bad chunk size, no overlap, wrong top-k, no score threshold. Step 09 is devoted to this.

## Vocabulary cheat sheet

| Term | Plain meaning |
|------|---------------|
| Embedding / vector | A list of numbers that captures the meaning of text |
| Chunk | A small piece of a document |
| Cosine similarity | How close two vectors point (1 = same direction) |
| top-k | How many chunks to retrieve |
| Grounding | Forcing the answer to come from the provided text |
| Hallucination | The model confidently inventing facts |
| pgvector | PostgreSQL extension for storing and searching vectors |
| Bedrock | AWS service that hosts models like Titan and Nova behind one API |
