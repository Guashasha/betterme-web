using BetterMe.WebGui.Dtos;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BetterMe.WebGui.Services;

namespace MyApp.Namespace;

public class UserInfoModel : PageModel
{
    private readonly UsersApi _api;
    private readonly ILogger<UserInfoModel> _log;

    public UserInfoModel(UsersApi api, ILogger<UserInfoModel> log)
    {
        _api = api;
        _log = log;
    }

    public UserDto? Profile { get; private set; }

    public async Task OnGetAsync()
    {
        const string demoId = "680b2c756c6e0ff289da9140";

        try
        {
            Profile = await _api.GetByIdAsync(demoId, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "User API call failed");
            Profile = null;
        }
    }
}