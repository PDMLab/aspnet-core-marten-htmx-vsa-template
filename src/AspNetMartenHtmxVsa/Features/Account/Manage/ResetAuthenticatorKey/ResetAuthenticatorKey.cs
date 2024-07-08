using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.ResetAuthenticatorKey;

public class ResetAuthenticatorKeyController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ResetAuthenticatorKeyController(
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
    _logger = loggerFactory.CreateLogger<ResetAuthenticatorKeyController>();
  }


  //
  // POST: /Manage/ResetAuthenticatorKey
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ResetAuthenticatorKey()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
      _logger.LogInformation(1, "User reset authenticator key.");
    }

    return RedirectToAction(nameof(Index), "Manage");
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
