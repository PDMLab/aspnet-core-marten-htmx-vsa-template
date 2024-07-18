using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.ConfirmEmail;

public class ConfirmEmailController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public ConfirmEmailController(
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
    _logger = loggerFactory.CreateLogger<ConfirmEmailController>();
  }


  // GET: /Account/ConfirmEmail
  [HttpGet]
  [AllowAnonymous]
  public async Task<IActionResult> ConfirmEmail(
    string userId,
    string code
  )
  {
    if (userId == null || code == null)
    {
      return View("Error");
    }

    var user = await _userManager.FindByIdAsync(userId);
    if (user == null)
    {
      return View("Error");
    }

    var result = await _userManager.ConfirmEmailAsync(user, code);
    return View(result.Succeeded ? "ConfirmEmail" : "Error");
  }
}
