using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.DisableTwoFactorAuthentication;

public class DisableTwoFactorAuthenctionController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public DisableTwoFactorAuthenctionController(
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
    _logger = loggerFactory.CreateLogger<DisableTwoFactorAuthenctionController>();
  }

  //
  // POST: /Manage/DisableTwoFactorAuthentication
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DisableTwoFactorAuthentication()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      await _userManager.SetTwoFactorEnabledAsync(user, false);
      await _signInManager.SignInAsync(user, isPersistent: false);
      _logger.LogInformation(2, "User disabled two-factor authentication.");
    }

    return RedirectToAction(nameof(Index), "Manage");
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
