using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Features.Account.Manage.ManageAccount;
using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.VerifyPhoneNumber;

public class VerifyPhoneNumberViewModel
{
  [Required] public string Code { get; set; }

  [Required]
  [Phone]
  [Display(Name = "Phone number")]
  public string PhoneNumber { get; set; }
}

[Authorize]
public class VerifyPhoneNumberController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public VerifyPhoneNumberController(
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
    _logger = loggerFactory.CreateLogger<VerifyPhoneNumberController>();
  }


  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }


  [HttpGet("/account/verify-phone-number")]
  public async Task<IActionResult> VerifyPhoneNumber(
    string phoneNumber
  )
  {
    var code = await _userManager.GenerateChangePhoneNumberTokenAsync(await GetCurrentUserAsync(), phoneNumber);
    // Send an SMS to verify the phone number
    return phoneNumber == null
      ? View("Error")
      : View(
        "~/Features/Account/Manage/VerifyPhoneNumber/VerifyPhoneNumber.cshtml",
        new VerifyPhoneNumberViewModel
        {
          PhoneNumber = phoneNumber
        }
      );
  }

  [HttpPost("/account/verify-phone-number")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> VerifyPhoneNumber(
    VerifyPhoneNumberViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View(
        "~/Features/Account/Manage/VerifyPhoneNumber/VerifyPhoneNumber.cshtml",
        model
      );
    }

    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var result = await _userManager.ChangePhoneNumberAsync(
        user,
        model.PhoneNumber,
        model.Code
      );
      if (result.Succeeded)
      {
        await _signInManager.SignInAsync(user, isPersistent: false);
        // TODO: HTMX response
        return RedirectToAction(
          nameof(ManageAccountController.ManageAccount),
          "ManageAccount",
          new
          {
            Message = ManageMessageId.AddPhoneSuccess
          }
        );
      }
    }

    // If we got this far, something failed, redisplay the form
    ModelState.AddModelError(string.Empty, "Failed to verify phone number");
    return View(
      "~/Features/Account/Manage/VerifyPhoneNumber/VerifyPhoneNumber.cshtml",
      model
    );
  }
}
