namespace BetterMe.WebGui.Classes;
public class FeedItem
{
    public string Id { get; set; }           // post.Id
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string ImageDataUrl { get; set; } // "data:image/jpeg;base64,…"
}