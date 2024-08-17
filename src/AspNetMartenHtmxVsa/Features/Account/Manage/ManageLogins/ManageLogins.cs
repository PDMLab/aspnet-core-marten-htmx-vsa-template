using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;

public class ManageLoginsViewModel
{
  public IList<UserLoginInfo> CurrentLogins { get; set; }

  public IList<AuthenticationScheme> OtherLogins { get; set; }
}

public enum ManageMessageId
{
  AddPhoneSuccess,
  AddLoginSuccess,
  ChangePasswordSuccess,
  SetTwoFactorSuccess,
  SetPasswordSuccess,
  RemoveLoginSuccess,
  RemovePhoneSuccess,
  Error
}

[Authorize]
public class ManageLoginsController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ManageLoginsController(
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
    _logger = loggerFactory.CreateLogger<ManageLoginsController>();
  }

  [HttpGet("/account/manage-logins")]
  public async Task<IActionResult> ManageLogins(
    ManageMessageId? message = null
  )
  {
    ViewData["StatusMessage"] =
      message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
      : message == ManageMessageId.AddLoginSuccess ? "The external login was added."
      : message == ManageMessageId.Error ? "An error has occurred."
      : "";
    var user = await GetCurrentUserAsync();
    if (user == null)
    {
      return View("Error");
    }

    var userLogins = await _userManager.GetLoginsAsync(user);
    var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
    var otherLogins = schemes.Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider))
      .ToList();
    ViewData["ShowRemoveButton"] = user.PasswordHash != null || userLogins.Count > 1;
    return View(
      "~/Features/Account/Manage/ManageLogins/ManageLogins.cshtml",
      new ManageLoginsViewModel
      {
        CurrentLogins = userLogins,
        OtherLogins = otherLogins
      }
    );
  }


  [HttpPost("/account/manage-logins")]
  [ValidateAntiForgeryToken]
  public IActionResult LinkLogin(
    string provider
  )
  {
    // Request a redirect to the external login provider to link a login for the current user
    var redirectUrl = Url.Action("LinkLoginCallback", "ManageLogins");
    var properties = _signInManager.ConfigureExternalAuthenticationProperties(
      provider,
      redirectUrl,
      _userManager.GetUserId(User)
    );
    return Challenge(properties, provider);
  }

  [HttpGet("/account/manage-logins/login-callback")]
  public async Task<ActionResult> LinkLoginCallback()
  {
    var user = await GetCurrentUserAsync();
    if (user == null)
    {
      return View("Error");
    }

    var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
    if (info == null)
    {
      return RedirectToAction(
        nameof(ManageLogins),
        new
        {
          Message = ManageMessageId.Error
        }
      );
    }

    var result = await _userManager.AddLoginAsync(user, info);
    var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;
    return RedirectToAction(
      nameof(ManageLogins),
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
