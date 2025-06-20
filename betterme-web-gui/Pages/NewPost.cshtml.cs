using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultimediaService;
using MyApp.Helpers;

namespace MyApp.Namespace
{
    public class NewPostModel : PageModel
    {
        private readonly MultimediaService.MultimediaService.MultimediaServiceClient _grpc;

        public string UserIdToken { get; private set; } = "";

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
            {
                Console.WriteLine("No cookie sent, redirecting to Login");
                return RedirectToPage("/Login");
            }
            Console.WriteLine($"Got cookie: {token.Substring(0,10)}…");
            try
            {
                Console.WriteLine(" Validating token…");
                var jwt = JwtUtils.ValidateAndDecode(token);
                Console.WriteLine("Token valid. Claims:");
                foreach (var c in jwt.Claims)
                    Console.WriteLine($"   • {c.Type} = {c.Value}");

                var id = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                Console.WriteLine($"Extracted id = '{id}'");
                if (string.IsNullOrEmpty(id))
                {
                    return RedirectToPage("/Login");
                }

                UserIdToken = id;
            }
            catch (Exception ex)
            {
                // Response.Cookies.Delete("accessToken");
                return RedirectToPage("/Login");
            }

            var createReq = new Post
            {
                Title       = Title,
                Description = Description,
                Category    = Category,
                UserId      = UserIdToken,
                TimeStamp   = Timestamp.FromDateTime(DateTime.UtcNow),
                Status      = "Published"
            };
            Post created = await _grpc.CreatePostAsync(createReq);

            if (FileUpload is { Length: > 0 })
            {
                using var call = _grpc.UploadPostMultimedia();

                var resourceId = created.Id;
                var ext        = Path.GetExtension(FileUpload.FileName).TrimStart('.');

                using var fs = FileUpload.OpenReadStream();
                var buffer = new byte[64 * 1024];
                int bytesRead;
                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await call.RequestStream.WriteAsync(new FileChunk
                    {
                        ResourceId = resourceId,
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
