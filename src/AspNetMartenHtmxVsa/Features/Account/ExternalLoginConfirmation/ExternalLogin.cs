using System.Security.Claims;
using AspNetMartenHtmxVsa.Features.Account.Services;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.ExternalLoginConfirmation;

public class ExternalLoginController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ExternalLoginController(
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
    _logger = loggerFactory.CreateLogger<ExternalLoginController>();
  }


  //
  // POST: /Account/ExternalLogin
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public IActionResult ExternalLogin(
    string provider,
    string returnUrl = null
  )
  {
    // Request a redirect to the external login provider.
    var redirectUrl = Url.Action(
      "ExternalLoginCallback",
      "ExternalLogin",
      new
      {
        ReturnUrl = returnUrl
      }
    );
    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
    return Challenge(properties, provider);
  }

  //
  // GET: /Account/ExternalLoginCallback
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> ExternalLoginCallback(
    string returnUrl = null,
    string remoteError = null
  )
  {
    if (remoteError != null)
    {
      ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
      return View("~/Features/Account/Login/Login.cshtml");
    }

    var info = await _signInManager.GetExternalLoginInfoAsync();
    if (info == null)
    {
      return RedirectToAction(nameof(Login), "Login");
    }

    // Sign in the user with this external login provider if the user already has a login.
    var result = await _signInManager.ExternalLoginSignInAsync(
      info.LoginProvider,
      info.ProviderKey,
      isPersistent: false
    );
    if (result.Succeeded)
    {
      // Update any authentication tokens if login succeeded
      await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

      _logger.LogInformation(
        5,
        "User logged in with {Name} provider.",
        info.LoginProvider
      );
      return RedirectToLocal(returnUrl);
    }

    if (result.RequiresTwoFactor)
    {
      return RedirectToAction(
        nameof(SendCode),
        "SendCode",
        new
        {
          ReturnUrl = returnUrl
        }
      );
    }

    if (result.IsLockedOut)
    {
      return View("~/Features/Account/Lockout/Lockout.cshtml");
    }
    else
    {
      // If the user does not have an account, then ask the user to create an account.
      ViewData["ReturnUrl"] = returnUrl;
      ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
      var email = info.Principal.FindFirstValue(ClaimTypes.Email);
      return View(
        "~/Features/Account/ExternalLoginConfirmation/ExternalLoginConfirmation.cshtml",
        new ExternalLoginConfirmationViewModel
        {
          Email = email
        }
      );
    }
  }

  //
  // POST: /Account/ExternalLoginConfirmation
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ExternalLoginConfirmation(
    ExternalLoginConfirmationViewModel model,
    string returnUrl = null
  )
  {
    if (ModelState.IsValid)
    {
      // Get the information about the user from the external login provider
      var info = await _signInManager.GetExternalLoginInfoAsync();
      if (info == null)
      {
        return View("~/Features/Account/ExternalLoginConfirmation/ExternalLoginFailure.cshtml");
      }

      var user = new AppUser
      {
        UserName = model.Email,
        Email = model.Email
      };
      var result = await _userManager.CreateAsync(user);
      if (result.Succeeded)
      {
        result = await _userManager.AddLoginAsync(user, info);
        if (result.Succeeded)
        {
          await _signInManager.SignInAsync(user, isPersistent: false);
          _logger.LogInformation(
            6,
            "User created an account using {Name} provider.",
            info.LoginProvider
          );

          // Update any authentication tokens as well
          await _signInManager.UpdateExternalAuthenticationTokensAsync(info);

          return RedirectToLocal(returnUrl);
        }
      }

      AddErrors(result);
    }

    ViewData["ReturnUrl"] = returnUrl;
    return View("~/Features/Account/ExternalLoginConfirmation/ExternalLoginConfirmation.cshtml", model);
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

  private void AddErrors(
    IdentityResult result
  )
  {
    foreach (var error in result.Errors)
    {
      ModelState.AddModelError(string.Empty, error.Description);
    }
  }
}
