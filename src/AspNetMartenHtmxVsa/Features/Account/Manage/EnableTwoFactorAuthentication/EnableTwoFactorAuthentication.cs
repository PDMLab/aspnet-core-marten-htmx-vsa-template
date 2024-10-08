using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.EnableTwoFactorAuthentication;

[Authorize]
public class EnableTwoFactorAuthenticationController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public EnableTwoFactorAuthenticationController(
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
    _logger = loggerFactory.CreateLogger<EnableTwoFactorAuthenticationController>();
  }


  [HttpPost("/account/enable-twofactor")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EnableTwoFactorAuthentication()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      await _userManager.SetTwoFactorEnabledAsync(user, true);
      await _signInManager.SignInAsync(user, isPersistent: false);
      _logger.LogInformation(1, "User enabled two-factor authentication.");
    }

    return RedirectToAction(nameof(ManageAccount), "ManageAccount");
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
