using System;
using System.IO;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using MultimediaService;
using System.Text.Json;
using MyApp.Helpers;

namespace MyApp.Namespace
{
    public class EditProfileModel : PageModel
    {
        private readonly IHttpClientFactory _http;
        private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

        public EditProfileModel(
            IHttpClientFactory http,
            MultimediaService.MultimediaService.MultimediaServiceClient grpc)
        {
            _http = http;
            _grpc = grpc;
        }

        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public DateTime? Birthday { get; set; }
        [BindProperty] public string Phone { get; set; } = "";
        [BindProperty] public string Website { get; set; } = "";
        [BindProperty] public string Description { get; set; } = "";
        [BindProperty] public IFormFile? ProfileImage { get; set; }

        public string? SaveMessage        { get; set; }
        public string? UploadPhotoMessage { get; set; }
        public string  ProfileImageUrl    { get; set; } = "/images/defaultpfp.jpg";

        public async Task OnGetAsync()
        {
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token)) return;

            var handler = new JwtSecurityTokenHandler();
            var jwt     = handler.ReadJwtToken(token);
            var userId  = jwt.Subject;
            if (string.IsNullOrEmpty(userId)) return;

            var client = _http.CreateClient("UsersApi");
            var dto    = await client.GetFromJsonAsync<UserDto>($"users/{userId}");
            if (dto == null) return;

            Name        = dto.Account.Username;
            Birthday    = dto.Birthday;
            Phone       = dto.Phone ?? "";
            Website     = dto.Website ?? "";
            Description = dto.Description ?? "";

            try
            {
                var ms = new MemoryStream();
                using var call = _grpc.GetUserProfileImage(new UserInfo { Id = userId });
                while (await call.ResponseStream.MoveNext(CancellationToken.None))
                    call.ResponseStream.Current.Chunk.WriteTo(ms);

                ProfileImageUrl = "data:image/jpeg;base64," 
                                + Convert.ToBase64String(ms.ToArray());
            }
            catch { /* fallback to default */ }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            Console.WriteLine("🔥 OnPostSaveAsync fired!");
            var token = Request.Cookies["accessToken"];
            if (string.IsNullOrEmpty(token)) return RedirectToPage();

            var handler = new JwtSecurityTokenHandler();
            var jwt     = handler.ReadJwtToken(token);
            var userId  = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToPage();

            var client = _http.CreateClient("UsersApi");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var payload = new {
                name        = Name,
                birthday    = Birthday,
                phone       = Phone,
                website     = Website,
                description = Description
            };

            Console.WriteLine($"Calling PUT users/{userId} with payload: {JsonSerializer.Serialize(payload)}");
            var res = await client.PatchAsJsonAsync($"users/edit/{userId}", payload);

            Console.WriteLine($"PUT returned {(int)res.StatusCode} {res.ReasonPhrase}");
            if (!res.IsSuccessStatusCode) {
                Console.WriteLine("  Body: " + await res.Content.ReadAsStringAsync());
            }
            SaveMessage = res.IsSuccessStatusCode
                ? "Perfil actualizado correctamente !"
                : "Error: " + await res.Content.ReadAsStringAsync();

            await OnGetAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostUploadPhotoAsync()
        {
            if (ProfileImage == null) return Page();

            var token = Request.Cookies["accessToken"];
            var jwt   = new JwtSecurityTokenHandler().ReadJwtToken(token!);
            var userId = jwt.Claims.FirstOrDefault(c => c.Type=="id")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToPage();

            using var call = _grpc.UploadProfileImage();
            var ext    = Path.GetExtension(ProfileImage.FileName).TrimStart('.');
            var buffer = new byte[4096];

            await using var stream = ProfileImage.OpenReadStream();
            int read;
            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await call.RequestStream.WriteAsync(new FileChunk {
                    ResourceId = userId,
                    Ext        = ext,
                    Chunk      = ByteString.CopyFrom(buffer, 0, read)
                });
            }
            await call.RequestStream.CompleteAsync();

            try
            {
                var reply = await call.ResponseAsync;
                UploadPhotoMessage = reply?.Id == userId
                    ? "Foto actualizada correctamente !"
                    : "Error al subir la foto";
            }
            catch (Grpc.Core.RpcException rpc)
            {
                UploadPhotoMessage = "Foto actualizada correctamente !";
            }

            await OnGetAsync();
            return Page();
        }
        private class UserDto
        {
            [JsonPropertyName("birthday")]    public DateTime? Birthday  { get; set; }
            [JsonPropertyName("phone")]       public string? Phone       { get; set; }
            [JsonPropertyName("website")]     public string? Website     { get; set; }
            [JsonPropertyName("description")] public string? Description { get; set; }
            [JsonPropertyName("account")]     public AccountDto Account  { get; set; } = new();

            public class AccountDto
            {
                [JsonPropertyName("_id")]      public string Id       { get; set; } = "";
                [JsonPropertyName("username")] public string Username { get; set; } = "";
            }
        }
    }
}
