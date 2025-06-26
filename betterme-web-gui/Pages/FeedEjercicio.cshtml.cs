using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultimediaService;
using BetterMe.WebGui.Classes;
using Grpc.Core;

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
            var postsClient = _http.CreateClient("PostsApi");
            var usersClient = _http.CreateClient("UsersApi");

            var url = string.IsNullOrEmpty(category)
                ? "posts"
                : $"posts?category={Uri.EscapeDataString(category)}";

            var posts = await postsClient.GetFromJsonAsync<List<PostDto>>(url)
                        ?? new();

            foreach (var p in posts)
            {
                var ms = new MemoryStream();
                using var call = _grpc.GetPostMultimedia(new PostInfo { Id = p.Id });
                while (await call.ResponseStream.MoveNext())
                    call.ResponseStream.Current.Chunk.WriteTo(ms);

                // GET /users/{id}
                var userDto = await usersClient
                    .GetFromJsonAsync<UserDto>($"users/{p.UserId}");

                Posts.Add(new FeedItem
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Category = p.Category,
                    ImageDataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray()),

                    UserName = userDto?.Account?.Username ?? "Unknown",
                    IsVerified = userDto?.Verified ?? false
                });
            }
        }

        public class PostDto
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public string UserId { get; set; }
        }
    
        private class UserDto
        {
            public bool Verified { get; set; }
            public AccountDto Account { get; set; } = new();
            public class AccountDto { public string Username { get; set; } = ""; }
        }
    }
}
