using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.ResetAuthenticatorKey;

[Authorize]
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


  [HttpPost("/account/reset-authenticator-key")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ResetAuthenticatorKey()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
      _logger.LogInformation(1, "User reset authenticator key.");
    }

    return RedirectToAction(nameof(ManageAccount), "ManageAccount");
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
