# RAG pipeline overview

An LLM only knows its training data, so it cannot answer from private documents and may fabricate an answer. Retrieval-Augmented Generation retrieves the relevant passages from the documents and instructs the model to answer only from them. Pasting entire documents into every prompt is too long, slow and costly.

## Ingestion (once per document)

```
PDF in S3 -> extract text per page -> chunk -> embed each chunk -> store in PostgreSQL
```

1. **Extract.** Read the PDF page by page, keeping the page number with each text span (required for citations).
2. **Chunk.** Split text into roughly 500-1000 character pieces with a small overlap between neighbors, so content cut at a boundary remains retrievable. Whole documents match too coarsely; single sentences lose context.
3. **Embed.** Send each chunk to Amazon Titan, which returns a 1024-dimension vector. Semantically similar texts produce nearby vectors even without shared words.
4. **Store.** Persist chunk text, page number, source document and vector in PostgreSQL. pgvector provides the `vector` type and nearest-neighbor search.

## Query (every question)

```
Question -> embed -> nearest chunks -> build prompt -> LLM -> answer + sources
```

1. Embed the question with the same model used at ingestion.
2. Retrieve the top-k chunks by cosine distance.
3. Build a prompt instructing the model to answer only from the supplied context and to say so when the answer is absent.
4. Generate the answer with Nova 2 Lite.
5. Return the answer with the file name and page number of each chunk used.

## Example

Page 7 of `policy.pdf` states: "Customers may return items within 30 days for a full refund." Ingestion stores that chunk with page 7 and its vector. The question "Can I get my money back after three weeks?" shares no keywords with it, but the vectors are close, so the chunk is retrieved and the answer cites `policy.pdf`, page 7.

## Retrieval quality

The model can only use what retrieval returns. Most quality problems come from retrieval: chunk size, overlap, top-k, and the absence of a similarity threshold. Step 09 covers tuning and evaluation.

## Glossary

| Term | Definition |
|------|------------|
| Embedding | Fixed-length numeric vector representing the meaning of a text |
| Chunk | A bounded piece of a document |
| Cosine similarity | Similarity of two vectors by angle; 1 means same direction |
| top-k | Number of chunks retrieved per query |
| Grounding | Constraining the answer to the supplied context |
| Hallucination | Model output not supported by the context |
| pgvector | PostgreSQL extension for vector storage and search |
| Bedrock | AWS service exposing foundation models such as Titan and Nova |
