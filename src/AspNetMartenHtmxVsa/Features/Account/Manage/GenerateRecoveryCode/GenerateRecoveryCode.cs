using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.GenerateRecoveryCode;

public class DisplayRecoveryCodesViewModel
{
  [Required] public IEnumerable<string> Codes { get; set; }
}

[Authorize]
public class GenerateRecoveryCodeController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public GenerateRecoveryCodeController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IEmailSender emailSender,
    ISmsSender smsSender,
    ILoggerFactory loggerFactory
  )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _emailSender = emailSender;
    _smsSender = smsSender;
    _logger = loggerFactory.CreateLogger<GenerateRecoveryCodeController>();
  }


  [HttpPost("/account/generate-recovery-code")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> GenerateRecoveryCode()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 5);
      _logger.LogInformation(1, "User generated new recovery code.");
      return View(
        "~/Features/Account/Manage/GenerateRecoveryCode/DisplayRecoveryCodes.cshtml",
        new DisplayRecoveryCodesViewModel
        {
          Codes = codes
        }
      );
    }

    return View("Error");
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
