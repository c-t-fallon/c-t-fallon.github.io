using BlazorApp.Models;

namespace BlazorApp.Services;

public class ContentService
{
    public IReadOnlyList<FeedItem> Items { get; } = new List<FeedItem>
    {
        // Posts: Kind = FeedItemKind.Post, Href = "/posts/{slug}"
        // Tools: Kind = FeedItemKind.Tool, Href = route of the tool page
        new()
        {
            Title = "beam analysis",
            Description = "Shear and moment diagrams for simple beams using singularity functions.",
            Date = new DateOnly(2026, 5, 2),
            Kind = FeedItemKind.Tool,
            Href = "/tools/beam-analysis",
            Tags = ["structural", "beams"],
        },
        new()
        {
            Title = "exploring fluxor: redux-style state in blazor",
            Description = "Coming from WPF/MVVM and Revit add-in development, I look at what Fluxor brings to Blazor state management and where the mental model shift lands.",
            Date = new DateOnly(2026, 5, 2),
            Kind = FeedItemKind.Post,
            Href = "/posts/fluxor",
        },
        new()
        {
            Title = "intro",
            Description = "What this site is and who's behind it.",
            Date = new DateOnly(2026, 4, 30),
            Kind = FeedItemKind.Post,
            Href = "/posts/intro",
        },
    };

    public IEnumerable<FeedItem> Feed => Items.OrderByDescending(x => x.Date);
    public IEnumerable<FeedItem> Tools => Items.Where(x => x.Kind == FeedItemKind.Tool).OrderByDescending(x => x.Date);
}
