using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.ResetPassword;

public class ResetPasswordViewModel
{
  [Required] [EmailAddress] public string Email { get; set; }

  [Required]
  [StringLength(
    100,
    ErrorMessage = "The {0} must be at least {2} characters long.",
    MinimumLength = 6
  )]
  [DataType(DataType.Password)]
  public string Password { get; set; }

  [DataType(DataType.Password)]
  [Display(Name = "Confirm password")]
  [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; }

  public string Code { get; set; }
}

public class ResetPasswordController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ResetPasswordController(
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
    _logger = loggerFactory.CreateLogger<ResetPasswordController>();
  }


  //
  // GET: /Account/ResetPassword
  [HttpGet]
  [AllowAnonymous]
  public IActionResult ResetPassword(
    string code = null
  )
  {
    return code == null
      ? View("Error")
      : View(
        "~/Features/Account/ResetPassword/ResetPassword.cshtml"
      );
  }

  //
  // POST: /Account/ResetPassword
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ResetPassword(
    ResetPasswordViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/ResetPassword/ResetPassword.cshtml", model);
    }

    var user = await _userManager.FindByEmailAsync(model.Email);
    if (user == null)
    {
      // Don't reveal that the user does not exist
      return RedirectToAction(nameof(ResetPasswordController.ResetPasswordConfirmation), "Account");
    }

    var result = await _userManager.ResetPasswordAsync(
      user,
      model.Code,
      model.Password
    );
    if (result.Succeeded)
    {
      return RedirectToAction(nameof(ResetPasswordController.ResetPasswordConfirmation), "Account");
    }

    AddErrors(result);
    return View("~/Features/Account/ResetPassword/ResetPassword.cshtml", model);
  }

  //
  // GET: /Account/ResetPasswordConfirmation
  [HttpGet]
  [AllowAnonymous]
  public IActionResult ResetPasswordConfirmation()
  {
    return View("~/Features/Account/ResetPassword/ResetPasswordConfirmation.cshtml");
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
