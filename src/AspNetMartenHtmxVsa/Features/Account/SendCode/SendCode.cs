using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNetMartenHtmxVsa.Features.Account.SendCode;

public class SendCodeViewModel
{
  public string SelectedProvider { get; set; }

  public ICollection<SelectListItem> Providers { get; set; }

  public string ReturnUrl { get; set; }

  public bool RememberMe { get; set; }
}

public class SendCodeController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public SendCodeController(
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
    _logger = loggerFactory.CreateLogger<SendCodeController>();
  }


  //
  // GET: /Account/SendCode
  [HttpGet]
  [AllowAnonymous]
  public async Task<ActionResult> SendCode(
    string returnUrl = null,
    bool rememberMe = false
  )
  {
    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user == null)
    {
      return View("Error");
    }

    var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
    var factorOptions = userFactors.Select(
        purpose => new SelectListItem
        {
          Text = purpose,
          Value = purpose
        }
      )
      .ToList();
    return View(
      "~/Features/Account/SendCode/SendCode.cshtml",
      new SendCodeViewModel
      {
        Providers = factorOptions,
        ReturnUrl = returnUrl,
        RememberMe = rememberMe
      }
    );
  }

  //
  // POST: /Account/SendCode
  [HttpPost]
  [AllowAnonymous]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> SendCode(
    SendCodeViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/SendCode/SendCode.cshtml", model);
    }

    var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
    if (user == null)
    {
      return View("Error");
    }

    if (model.SelectedProvider == "Authenticator")
    {
      return RedirectToAction(
        nameof(VerifyAuthenticatorCode),
        new
        {
          ReturnUrl = model.ReturnUrl,
          RememberMe = model.RememberMe
        }
      );
    }

    // Generate the token and send it
    var code = await _userManager.GenerateTwoFactorTokenAsync(user, model.SelectedProvider);
    if (string.IsNullOrWhiteSpace(code))
    {
      return View("Error");
    }

    var message = "Your security code is: " + code;
    if (model.SelectedProvider == "Email")
    {
      await _emailSender.SendEmailAsync(
        await _userManager.GetEmailAsync(user),
        "Security Code",
        message
      );
    }
    else if (model.SelectedProvider == "Phone")
    {
      await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
    }

    return RedirectToAction(
      nameof(VerifyCode),
      new
      {
        Provider = model.SelectedProvider,
        ReturnUrl = model.ReturnUrl,
        RememberMe = model.RememberMe
      }
    );
  }
}
