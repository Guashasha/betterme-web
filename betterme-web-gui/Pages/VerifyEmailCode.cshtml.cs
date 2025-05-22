using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class VerifyEmailCodeModel : PageModel
    {
        public string Email { get; private set; } = "";
        public void OnGet()
        {
            Email = Request.Query["email"].ToString();
        }
    }
}
