using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.VerifyAuthenticatorCode;

public class VerifyAuthenticatorCodeViewModel
{
  [Required] public string Code { get; set; }

  public string ReturnUrl { get; set; }

  [Display(Name = "Remember this browser?")]
  public bool RememberBrowser { get; set; }

  [Display(Name = "Remember me?")] public bool RememberMe { get; set; }
}

[Authorize]
public class VerifyAuthenticatorCodeController : Controller
{
  private readonly SignInManager<AppUser> _signInManager;
  private readonly ILogger _logger;

  public VerifyAuthenticatorCodeController(
    SignInManager<AppUser> signInManager,
    ILoggerFactory loggerFactory
  )
  {
    _signInManager = signInManager;
    _logger = loggerFactory.CreateLogger<VerifyAuthenticatorCodeController>();
  }


  //
  // GET: /Account/VerifyAuthenticatorCode
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> VerifyAuthenticatorCode(
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
      "~/Features/Account/VerifyAuthenticatorCode/VerifyAuthenticatorCode.cshtml",
      new VerifyAuthenticatorCodeViewModel
      {
        ReturnUrl = returnUrl,
        RememberMe = rememberMe
      }
    );
  }

  //
  // POST: /Account/VerifyAuthenticatorCode
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> VerifyAuthenticatorCode(
    VerifyAuthenticatorCodeViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/VerifyAuthenticatorCode/VerifyAuthenticatorCode.cshtml", model);
    }

    // The following code protects for brute force attacks against the two factor codes.
    // If a user enters incorrect codes for a specified amount of time then the user account
    // will be locked out for a specified amount of time.
    var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(
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
      return View("~/Features/Account/Lockout/Lockout.cshtml");
    }
    else
    {
      ModelState.AddModelError(string.Empty, "Invalid code.");
      return View("~/Features/Account/VerifyAuthenticatorCode/VerifyAuthenticatorCode.cshtml", model);
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
