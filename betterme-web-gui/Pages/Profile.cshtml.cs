using System.Text.Json.Serialization;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultimediaService;
using BetterMe.WebGui.Classes;
using System.Net.Http.Json;
using Grpc.Core;

namespace MyApp.Namespace{

public class ProfileModel : PageModel
{
    private readonly IHttpClientFactory _http;
    private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

    public string? Error { get; private set; }
    public UserDto? User { get; private set; }
    public string ProfileImageUrl { get; private set; } = "/images/defaultpfp.jpg";
    public List<FeedItem> Posts { get; } = new();

    public ProfileModel(IHttpClientFactory http,
                        MultimediaService.MultimediaService.MultimediaServiceClient grpc)
    {
        _http = http;
        _grpc = grpc;
    }

    public async Task OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Error = "Missing user id.";
            return;
        }
        var usersClient = _http.CreateClient("UsersApi");
        var userResp = await usersClient.GetAsync($"users/{id}");
        if (userResp.StatusCode == System.Net.HttpStatusCode.NotFound)
        { Error = "User not found."; return; }

        User = await userResp.Content.ReadFromJsonAsync<UserDto>();
        if (User == null) { Error = "Bad user payload."; return; }
        try
        {
            using var pfpCall = _grpc.GetUserProfileImage(new UserInfo { Id = id });
            var ms = new MemoryStream();
            while (await pfpCall.ResponseStream.MoveNext())
                pfpCall.ResponseStream.Current.Chunk.WriteTo(ms);

            ProfileImageUrl = "data:image/jpeg;base64," +
                              Convert.ToBase64String(ms.ToArray());
        }
        catch (Grpc.Core.RpcException rpc) when (rpc.Status.Detail.Contains("no file"))
        {
            // keep default picture
        }

        var postsClient = _http.CreateClient("PostsApi");
        var postDtos = await postsClient
            .GetFromJsonAsync<List<PostDto>>($"posts/user/{id}") ?? new();

        foreach (var p in postDtos)
        {
            var ms = new MemoryStream();
            using var call = _grpc.GetPostMultimedia(new PostInfo { Id = p.Id });
            while (await call.ResponseStream.MoveNext())
                call.ResponseStream.Current.Chunk.WriteTo(ms);

            Posts.Add(new FeedItem
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Category = p.Category,
                ImageDataUrl = "data:image/jpeg;base64," +
                               Convert.ToBase64String(ms.ToArray())
            });
        }
    }

    public class PostDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class UserDto
    {
        public bool Verified { get; set; }
        public string Description { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Website { get; set; } = "";
        public DateTime? Birthday { get; set; }
        public DateTime CreatedAt { get; set; }

        public AccountDto Account { get; set; } = new();
        public class AccountDto
        {
            public string Username { get; set; } = "";
            public string Name { get; set; } = "";
        }
    }

}
}