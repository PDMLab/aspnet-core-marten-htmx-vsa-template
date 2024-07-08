using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using AspNetMartenHtmxVsa.Features.GetHome;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Register;

public class RegisterViewModel
{
  [Required]
  [EmailAddress]
  [Display(Name = "Email")]
  public string Email { get; set; }

  [Required]
  [StringLength(
    100,
    ErrorMessage = "The {0} must be at least {2} characters long.",
    MinimumLength = 6
  )]
  [DataType(DataType.Password)]
  [Display(Name = "Password")]
  public string Password { get; set; }

  [DataType(DataType.Password)]
  [Display(Name = "Confirm password")]
  [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; }
}

public class RegisterController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public RegisterController(
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
    _logger = loggerFactory.CreateLogger<RegisterController>();
  }


  //
  // GET: /Account/Register
  [HttpGet]
  [AllowAnonymous]
  public IActionResult Register(
    string returnUrl = null
  )
  {
    ViewData["ReturnUrl"] = returnUrl;
    return View("~/Features/Account/Register/Register.cshtml");
  }

  //
  // POST: /Account/Register
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Register(
    RegisterViewModel model,
    string returnUrl = null
  )
  {
    ViewData["ReturnUrl"] = returnUrl;
    if (ModelState.IsValid)
    {
      var user = new AppUser
      {
        UserName = model.Email,
        Email = model.Email
      };
      var result = await _userManager.CreateAsync(user, model.Password);
      if (result.Succeeded)
      {
        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
        // Send an email with this link
        //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
        //await _emailSender.SendEmailAsync(model.Email, "Confirm your account",
        //    "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation(3, "User created a new account with password.");
        return RedirectToLocal(returnUrl);
      }

      AddErrors(result);
    }

    // If we got this far, something failed, redisplay form
    return View("~/Features/Account/Register/Register.cshtml", model);
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
