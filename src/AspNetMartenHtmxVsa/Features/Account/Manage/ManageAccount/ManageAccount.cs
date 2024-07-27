using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.ManageAccount;

public class IndexViewModel
{
  public bool HasPassword { get; set; }

  public IList<UserLoginInfo> Logins { get; set; }

  public string PhoneNumber { get; set; }

  public bool TwoFactor { get; set; }

  public bool BrowserRemembered { get; set; }

  public string AuthenticatorKey { get; set; }
}

public class ManageAccountController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ManageAccountController(
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
    _logger = loggerFactory.CreateLogger<ManageAccountController>();
  }


//
// GET: /Manage/Index
  [HttpGet("/Account/Manage")]
  public async Task<IActionResult> ManageAccount(
    ManageMessageId? message = null
  )
  {
    ViewData["StatusMessage"] =
      message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
      : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
      : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
      : message == ManageMessageId.Error ? "An error has occurred."
      : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
      : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
      : "";

    var user = await GetCurrentUserAsync();
    var model = new IndexViewModel
    {
      HasPassword = await _userManager.HasPasswordAsync(user),
      PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
      TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
      Logins = await _userManager.GetLoginsAsync(user),
      BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
      AuthenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user)
    };
    return View("~/Features/Account/Manage/ManageAccount/ManageAccount.cshtml", model);
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
