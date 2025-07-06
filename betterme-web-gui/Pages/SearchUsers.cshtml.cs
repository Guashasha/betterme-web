using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultimediaService;

namespace MyApp.Namespace
{
    public class SearchUsersModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

        public SearchUsersModel(
            IHttpClientFactory http,
            MultimediaService.MultimediaService.MultimediaServiceClient grpc)
        {
            _http = http;
            _grpc = grpc;
        }

        // bind the ?q= querystring
        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        public List<SearchUserItem> Results { get; } = new();

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Query))
                return;

            var client = _http.CreateClient("UsersApi");
            // call /api/users/search?q=<Query>
            var dtoList = await client
                .GetFromJsonAsync<List<UserDto>>($"users/search?q={Uri.EscapeDataString(Query)}")
                ?? new List<UserDto>();

            foreach (var u in dtoList)
            {
            var userId = u.Account.Id;                // now coming from JSON _id
            var userName = u.Account.Username;

            // 1) fetch avatar via gRPC
            string dataUrl;
            try
            {
                var ms = new MemoryStream();
                using var call = _grpc.GetUserProfileImage(new UserInfo { Id = userId });
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                call.ResponseStream.Current.Chunk.WriteTo(ms);
                dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(ms.ToArray());
            }
            catch (Grpc.Core.RpcException ex)
                when (ex.Status.Detail.Contains("no file for"))
            {
                // fallback to default pfp
                dataUrl = "/images/defaultpfp.jpg";
            }

            Results.Add(new SearchUserItem {
                Id           = userId,
                UserName     = userName,
                IsVerified   = u.Verified,
                ImageDataUrl = dataUrl
            });
            }
        }
        private class UserDto
        {
            // the top‐level "verified" field
            [JsonPropertyName("verified")]
            public bool Verified { get; set; }

            // the nested account object
            [JsonPropertyName("account")]
            public AccountDto Account { get; set; } = new();

            public class AccountDto
            {
                // Mongo writes this field as "_id"
                [JsonPropertyName("_id")]
                public string Id { get; set; } = "";

                // it already comes through as "username"
                [JsonPropertyName("username")]
                public string Username { get; set; } = "";
            }
        }
    }

    public class SearchUserItem
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public bool IsVerified { get; set; }
        public string ImageDataUrl { get; set; } = "";
    }
}
