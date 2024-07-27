using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Features.Account;
using AspNetMartenHtmxVsa.Features.Account.Manage.ManageAccount;
using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace a;

public class ChangePasswordViewModel
{
  [Required]
  [DataType(DataType.Password)]
  [Display(Name = "Current password")]
  public string OldPassword { get; set; }

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

public class ChangePasswordController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ChangePasswordController(
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
    _logger = loggerFactory.CreateLogger<ChangePasswordController>();
  }


  //
  // GET: /Manage/ChangePassword
  [HttpGet]
  public IActionResult ChangePassword()
  {
    return View("~/Features/Account/Manage/ChangePassword/ChangePassword.cshtml");
  }

  //
  // POST: /Manage/ChangePassword
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ChangePassword(
    ChangePasswordViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/Manage/ChangePassword/ChangePassword.cshtml", model);
    }

    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var result = await _userManager.ChangePasswordAsync(
        user,
        model.OldPassword,
        model.NewPassword
      );
      if (result.Succeeded)
      {
        await _signInManager.SignInAsync(user, isPersistent: false);
        _logger.LogInformation(3, "User changed their password successfully.");
        // TODO: HTMX response
        return RedirectToAction(
          nameof(ManageAccountController.ManageAccount),
          "ManageAccount",
          new
          {
            Message = ManageMessageId.ChangePasswordSuccess
          }
        );
      }

      AddErrors(result);
      return View("~/Features/Account/Manage/ChangePassword/ChangePassword.cshtml", model);
    }

    // TODO: HTMX response
    return RedirectToAction(
      nameof(ManageAccountController.ManageAccount),
      "ManageAccount",
      new
      {
        Message = ManageMessageId.Error
      }
    );
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
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
