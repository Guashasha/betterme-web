namespace BetterMe.WebGui.Classes;
public class FeedItem
{
    public string Id            { get; set; }   
    public string Title         { get; set; }
    public string Description   { get; set; }
    public string Category      { get; set; }
    public string ImageDataUrl  { get; set; }
    public string UserName      { get; set; }   
    public bool   IsVerified    { get; set; }   
}