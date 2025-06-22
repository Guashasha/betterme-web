using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultimediaService;
using BetterMe.WebGui.Classes;

namespace MyApp.Namespace
{
    public class FeedEjercicioModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

        public List<FeedItem> Posts { get; private set; } = new();

        public FeedEjercicioModel(
            IHttpClientFactory http,
            MultimediaService.MultimediaService.MultimediaServiceClient grpc)
        {
            _http = http;
            _grpc = grpc;
        }

            public async Task OnGetAsync(string? category)
            {
                var client = _http.CreateClient("PostsApi");
                var url = string.IsNullOrEmpty(category)
                ? "posts"
                : $"posts?category={Uri.EscapeDataString(category)}";

                var posts = await client.GetFromJsonAsync<List<PostDto>>(url)
                            ?? new List<PostDto>();

                //for each post stream the image via gRPC and base64‐ify it
                foreach (var p in posts)
                {
                    var ms = new MemoryStream();
                    using var call = _grpc.GetPostMultimedia(new PostInfo { Id = p.Id });
                    while (await call.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        var chunk = call.ResponseStream.Current;
                        chunk.Chunk.WriteTo(ms);
                    }
                    var dataUrl = "data:image/jpeg;base64," +
                        Convert.ToBase64String(ms.ToArray());

                    Posts.Add(new FeedItem
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Category = p.Category,
                        ImageDataUrl = dataUrl
                    });
                }
            }

        public class PostDto
        {
            public string Id          { get; set; }
            public string Title       { get; set; }
            public string Description { get; set; }
            public string Category    { get; set; }
        }
    
    }
}
