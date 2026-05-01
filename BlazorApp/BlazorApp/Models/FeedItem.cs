namespace BlazorApp.Models;

public enum FeedItemKind { Post, Tool }

public record FeedItem
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateOnly Date { get; init; }
    public required FeedItemKind Kind { get; init; }
    public string? Href { get; init; }
    public string[] Tags { get; init; } = [];
}
