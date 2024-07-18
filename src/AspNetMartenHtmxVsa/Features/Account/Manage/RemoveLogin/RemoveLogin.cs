using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.RemoveLogin;

public class RemoveLoginViewModel
{
  public string LoginProvider { get; set; }
  public string ProviderKey { get; set; }
}

public class RemoveLoginController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public RemoveLoginController(
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
    _logger = loggerFactory.CreateLogger<RemoveLoginController>();
  }


  //
  // POST: /Manage/RemoveLogin
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RemoveLogin(
    RemoveLoginViewModel account
  )
  {
    ManageMessageId? message = ManageMessageId.Error;
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var result = await _userManager.RemoveLoginAsync(
        user,
        account.LoginProvider,
        account.ProviderKey
      );
      if (result.Succeeded)
      {
        await _signInManager.SignInAsync(user, isPersistent: false);
        message = ManageMessageId.RemoveLoginSuccess;
      }
    }

    return RedirectToAction(
      nameof(ManageLogins),
      "ManageLogins",
      new
      {
        Message = message
      }
    );
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
