namespace MyChatBot.Api.Chunking;

/// <summary>
/// One piece of a document, ready to be embedded and stored. Maps onto the Chunk entity from
/// step 03 (PageNumber, ChunkIndex, Content); the embedding is added in step 05.
/// </summary>
public record TextChunk(int PageNumber, int ChunkIndex, string Content);
