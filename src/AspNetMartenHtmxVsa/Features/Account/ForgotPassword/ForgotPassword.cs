using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.ForgotPassword;

public class ForgotPasswordViewModel
{
  [Required] [EmailAddress] public string Email { get; set; }
}

[Authorize]
public class ForgotPasswordController : Controller
{
  private readonly UserManager<AppUser> _userManager;

  public ForgotPasswordController(
    UserManager<AppUser> userManager
  )
  {
    _userManager = userManager;
  }

  //
  // GET: /Account/ForgotPassword
  [HttpGet]
  [AllowAnonymous]
  public IActionResult ForgotPassword()
  {
    return View("~/Features/Account/ForgotPassword/ForgotPassword.cshtml");
  }

  //
  // POST: /Account/ForgotPassword
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ForgotPassword(
    ForgotPasswordViewModel model
  )
  {
    if (ModelState.IsValid)
    {
      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
      {
        // Don't reveal that the user does not exist or is not confirmed
        return View("~/Features/Account/ForgotPassword/ForgotPasswordConfirmation.cshtml");
      }

      // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=532713
      // Send an email with this link
      //var code = await _userManager.GeneratePasswordResetTokenAsync(user);
      //var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);
      //await _emailSender.SendEmailAsync(model.Email, "Reset Password",
      //   "Please reset your password by clicking here: <a href=\"" + callbackUrl + "\">link</a>");
      //return View("ForgotPasswordConfirmation");
    }

    // If we got this far, something failed, redisplay form
    return View("~/Features/Account/ForgotPassword/ForgotPassword.cshtml", model);
  }

  //
  // GET: /Account/ForgotPasswordConfirmation
  [HttpGet]
  [AllowAnonymous]
  public IActionResult ForgotPasswordConfirmation()
  {
    return View("~/Features/Account/ForgotPassword/ForgotPasswordConfirmation.cshtml");
  }
}
