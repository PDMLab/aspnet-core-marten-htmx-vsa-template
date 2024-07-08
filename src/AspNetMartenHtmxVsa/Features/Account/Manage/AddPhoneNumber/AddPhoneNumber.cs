using System.ComponentModel.DataAnnotations;
using AspNetMartenHtmxVsa.Areas.Identity.Data;
using AspNetMartenHtmxVsa.Features.Account.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Account.Manage.AddPhoneNumber;

public class AddPhoneNumberViewModel
{
  [Required]
  [Phone]
  [Display(Name = "Phone number")]
  public string PhoneNumber { get; set; }
}

public class AddPhoneNumberController : Controller
{
  private readonly UserManager<AppUser> _userManager;
  private readonly SignInManager<AppUser> _signInManager;
  private readonly IEmailSender _emailSender;
  private readonly ISmsSender _smsSender;
  private readonly ILogger _logger;

  public AddPhoneNumberController(
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
    _logger = loggerFactory.CreateLogger<AddPhoneNumberController>();
  }


  //
  // GET: /Manage/AddPhoneNumber
  public IActionResult AddPhoneNumber() => View("~/Features/Account/Manage/AddPhoneNumber/AddPhoneNumber.cshtml");

  //
  // POST: /Manage/AddPhoneNumber
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> AddPhoneNumber(
    AddPhoneNumberViewModel model
  )
  {
    if (!ModelState.IsValid)
    {
      return View("~/Features/Account/Manage/AddPhoneNumber/AddPhoneNumber.cshtml", model);
    }

    // Generate the token and send it
    var user = await GetCurrentUserAsync();
    var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.PhoneNumber);
    await _smsSender.SendSmsAsync(model.PhoneNumber, "Your security code is: " + code);
    return RedirectToAction(
      nameof(VerifyPhoneNumber),
      new
      {
        PhoneNumber = model.PhoneNumber
      }
    );
  }

  private Task<AppUser> GetCurrentUserAsync()
  {
    return _userManager.GetUserAsync(HttpContext.User);
  }
}
