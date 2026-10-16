namespace MyChatBot.Api.Pdf;

/// <summary>The text of one PDF page. PageNumber starts at 1, like the page numbers a reader sees.</summary>
public record PageText(int PageNumber, string Text);
