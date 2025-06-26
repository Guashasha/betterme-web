using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using MultimediaService;

namespace MyApp.Namespace
{
    public class NewPostModel : PageModel
    {
        private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

        [BindProperty] public string Title       { get; set; } = "";
        [BindProperty] public string Description { get; set; } = "";
        [BindProperty] public string Category    { get; set; } = "";
        [BindProperty] public IFormFile? FileUpload  { get; set; }

        public NewPostModel(MultimediaService.MultimediaService.MultimediaServiceClient grpc)
        {
            _grpc = grpc;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!Request.Cookies.TryGetValue("accessToken", out var token))
                return RedirectToPage("/Login");

            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims
                           .FirstOrDefault(c => c.Type == "id" || c.Type == "sub")
                           ?.Value;
            if (string.IsNullOrEmpty(userId))
                return RedirectToPage("/Login");

            var createReq = new Post
            {
                Title       = Title,
                Description = Description,
                Category    = Category,
                UserId      = userId,
                TimeStamp   = Timestamp.FromDateTime(DateTime.UtcNow),
                Status      = "Published"
            };
            var created = await _grpc.CreatePostAsync(createReq);

            if (FileUpload is { Length: > 0 })
            {
                using var call = _grpc.UploadPostMultimedia();
                var ext      = Path.GetExtension(FileUpload.FileName).TrimStart('.');
                var buffer   = new byte[64 * 1024];

                await using var fs = FileUpload.OpenReadStream();
                int bytesRead;
                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await call.RequestStream.WriteAsync(new FileChunk
                    {
                        ResourceId = created.Id,
                        Ext        = ext,
                        Chunk      = ByteString.CopyFrom(buffer, 0, bytesRead)
                    });
                }

                await call.RequestStream.CompleteAsync();
                _ = await call;  
            }

            return RedirectToPage("/NewPost");
        }
    }
}
