using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace betterme_web_gui.Pages.ProfileVerification;

public class VerificationRequestModel : PageModel
{
    private readonly ILogger<VerificationRequestModel> _logger;

    public VerificationRequestModel(ILogger<VerificationRequestModel> logger)
    {
        _logger = logger;
    }
}