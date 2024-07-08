using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.VerifyCode;

public class VerifyCodeViewModel
{
  [Required] public string Provider { get; set; }

  [Required] public string Code { get; set; }

  public string ReturnUrl { get; set; }

  [Display(Name = "Remember this browser?")]
  public bool RememberBrowser { get; set; }

  [Display(Name = "Remember me?")] public bool RememberMe { get; set; }
}

public class VerifyCodeController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public VerifyCodeController(
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
    _logger = loggerFactory.CreateLogger<VerifyCodeController>();
  }


  //
  // GET: /Account/VerifyCode
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> VerifyCode(
    string provider,
    bool rememberMe,
    string returnUrl = null
  )
  {
    // Require that the user has already logged in via username/password or external login
    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user == null)
    {
      return View("Error");
    }

    return View(
      "~/Features/Account/VerifyCode/VerifyCode.cshtml",
      new VerifyCodeViewModel
      {
        Provider = provider,
        ReturnUrl = returnUrl,
        RememberMe = rememberMe
      }
    );
  }

  //
  // POST: /Account/VerifyCode
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> VerifyCode(
    VerifyCodeViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View(
        "~/Features/Account/VerifyCode/VerifyCode.cshtml",
        model
      );
    }

    // The following code protects for brute force attacks against the two factor codes.
    // If a user enters incorrect codes for a specified amount of time then the user account
    // will be locked out for a specified amount of time.
    var result = await _signInManager.TwoFactorSignInAsync(
      model.Provider,
      model.Code,
      model.RememberMe,
      model.RememberBrowser
    );
    if (result.Succeeded)
    {
      return RedirectToLocal(model.ReturnUrl);
    }

    if (result.IsLockedOut)
    {
      _logger.LogWarning(7, "User account locked out.");
      return View(
        "~/Features/Account/Lockout/Lockout.cshtml"
      );
    }
    else
    {
      ModelState.AddModelError(string.Empty, "Invalid code.");
      return View(
        "~/Features/Account/VerifyCode/VerifyCode.cshtml",
        model
      );
    }
  }

  private IActionResult RedirectToLocal(
    string returnUrl
  )
  {
    if (Url.IsLocalUrl(returnUrl))
    {
      return Redirect(returnUrl);
    }
    else
    {
      return RedirectToAction(
        nameof(GetHomeController.GetHome),
        "GetHome",
        "Home"
      );
    }
  }
}
