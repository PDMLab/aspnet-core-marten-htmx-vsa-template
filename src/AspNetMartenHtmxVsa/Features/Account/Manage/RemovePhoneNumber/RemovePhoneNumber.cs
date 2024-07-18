using AspNetMartenHtmxVsa.Features.Account.Manage.ManageLogins;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.RemovePhoneNumber;

public class RemovePhoneNumberController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public RemovePhoneNumberController(
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
    _logger = loggerFactory.CreateLogger<RemovePhoneNumberController>();
  }


  //
  // GET: /Manage/RemovePhoneNumber
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RemovePhoneNumber()
  {
    var user = await GetCurrentUserAsync();
    if (user != null)
    {
      var result = await _userManager.SetPhoneNumberAsync(user, null);
      if (result.Succeeded)
      {
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction(
          nameof(RemovePhoneNumber),
          new
          {
            Message = ManageMessageId.RemovePhoneSuccess
          }
        );
      }
    }

    return RedirectToAction(
      nameof(RemovePhoneNumber),
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
}
