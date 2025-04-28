using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace betterme_web_gui.Pages;

public class CreateAccountModel : PageModel
{
    private readonly ILogger<CreateAccountModel> _logger;

    public CreateAccountModel(ILogger<CreateAccountModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {

    }
}