using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Features.Account.Services;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Login;

public class LoginViewModel
{
  [Microsoft.Build.Framework.Required]
  [EmailAddress]
  public string Email { get; set; }

  [Microsoft.Build.Framework.Required]
  [DataType(DataType.Password)]
  public string Password { get; set; }

  [Display(Name = "Remember me?")] public bool RememberMe { get; set; }
}

public class LoginController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public LoginController(
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
    _logger = loggerFactory.CreateLogger<LoginController>();
  }


  //
  // GET: /Account/Login
  [HttpGet]
  [AllowAnonymous]
  public IActionResult Login(
    string returnUrl = null
  )
  {
    ViewData["ReturnUrl"] = returnUrl;
    return View("~/Features/Account/Login/Login.cshtml");
  }


  //
  // POST: /Account/Login
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(
    LoginViewModel model,
    string returnUrl = null
  )
  {
    ViewData["ReturnUrl"] = returnUrl;
    if (ModelState.IsValid)
    {
      // This doesn't count login failures towards account lockout
      // To enable password failures to trigger account lockout, set lockoutOnFailure: true
      var result = await _signInManager.PasswordSignInAsync(
        model.Email,
        model.Password,
        model.RememberMe,
        lockoutOnFailure: false
      );
      if (result.Succeeded)
      {
        _logger.LogInformation(1, "User logged in.");
        return RedirectToLocal(returnUrl);
      }

      if (result.RequiresTwoFactor)
      {
        return RedirectToAction(
          nameof(SendCode),
          "SendCode",
          new
          {
            ReturnUrl = returnUrl,
            RememberMe = model.RememberMe
          }
        );
      }

      if (result.IsLockedOut)
      {
        _logger.LogWarning(2, "User account locked out.");
        return View("~/Features/Account/Lockout/Lockout.cshtml");
      }
      else
      {
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View("~/Features/Account/Login/Login.cshtml", model);
      }
    }

    // If we got this far, something failed, redisplay form
    return View("~/Features/Account/Login/Login.cshtml", model);
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
