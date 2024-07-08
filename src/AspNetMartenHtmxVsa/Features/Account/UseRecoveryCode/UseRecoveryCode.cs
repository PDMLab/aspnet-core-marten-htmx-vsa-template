using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.UseRecoveryCode;

public class UseRecoveryCodeViewModel
{
  [Required] public string Code { get; set; }

  public string ReturnUrl { get; set; }
}

public class UseRecoveryCodeController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly ILogger _logger;

  public UseRecoveryCodeController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ILoggerFactory loggerFactory
  )
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _logger = loggerFactory.CreateLogger<UseRecoveryCodeController>();
  }

  //
  // GET: /Account/UseRecoveryCode
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> UseRecoveryCode(
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
      "~/Features/Account/UseRecoveryCode/UseRecoveryCode.cshtml",
      new UseRecoveryCodeViewModel
      {
        ReturnUrl = returnUrl
      }
    );
  }

  //
  // POST: /Account/UseRecoveryCode
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> UseRecoveryCode(
    UseRecoveryCodeViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View(
        "~/Features/Account/UseRecoveryCode/UseRecoveryCode.cshtml",
        model
      );
    }

    var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(model.Code);
    if (result.Succeeded)
    {
      return RedirectToLocal(model.ReturnUrl);
    }
    else
    {
      ModelState.AddModelError(string.Empty, "Invalid code.");
      return View(
        "~/Features/Account/UseRecoveryCode/UseRecoveryCode.cshtml",
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
