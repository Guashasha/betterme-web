using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class NewAccountModel : PageModel
    {
        [BindProperty]
    [Required]
    public string FullName { get; set; } = "";

    [BindProperty]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty]
    [Required]
    public string Password { get; set; } = "";

    [BindProperty]
    [Required]
    [Compare("Password")]
    public string RepeatPassword { get; set; } = "";

    [BindProperty]
    [Required]
    public DateTime BirthDate { get; set; }

    [BindProperty]
    public string Gender { get; set; } = "Masculino";
        public void OnGet()
        {
        }
    }
}
