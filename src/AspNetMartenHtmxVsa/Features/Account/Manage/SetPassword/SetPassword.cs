using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.SetPassword;

public class SetPasswordViewModel
{
  [Required]
  [StringLength(
    100,
    ErrorMessage = "The {0} must be at least {2} characters long.",
    MinimumLength = 6
  )]
  [DataType(DataType.Password)]
  [Display(Name = "New password")]
  public string NewPassword { get; set; }

  [DataType(DataType.Password)]
  [Display(Name = "Confirm new password")]
  [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
  public string ConfirmPassword { get; set; }
}

[Authorize]
public class SetPasswordController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public SetPasswordController(
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
    _logger = loggerFactory.CreateLogger<SetPasswordController>();
  }

  //
  [HttpGet("/account/set-password")]
  public IActionResult SetPassword()
  {
    return View("~/Features/Account/Manage/SetPassword/SetPassword.cshtml");
  }

  [HttpPost("/account/set-password")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> SetPassword(
    SetPasswordViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/Manage/SetPassword/SetPassword.cshtml", model);
    }

    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
      if (result.Succeeded)
      {
        await _signInManager.SignInAsync(user, isPersistent: false);
        // TODO: HTMX response
        return RedirectToAction(
          nameof(ManageAccount),
          "ManageAccount",
          new
          {
            Message = ManageMessageId.SetPasswordSuccess
          }
        );
      }

      AddErrors(result);
      return View("~/Features/Account/Manage/SetPassword/SetPassword.cshtml", model);
    }

    // TODO: HTMX response
    return RedirectToAction(
      nameof(ManageAccount),
      "ManageAccount",
      new
      {
        Message = ManageMessageId.Error
      }
    );
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

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
