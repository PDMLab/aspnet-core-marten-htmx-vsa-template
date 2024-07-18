using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using AspNetMartenHtmxVsa.Core;
using AspNetMartenHtmxVsa.Features.Account;
using AspNetMartenHtmxVsa.Features.Subscriptions.SetCompanyNameFromUserRegistration;
using AspNetMartenHtmxVsa.Features.Subscriptions.Subscribe;
using Easy_Password_Validator;
using Easy_Password_Validator.Models;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Plugins;
using StoreOptions = Marten.StoreOptions;
using static AspNetMartenHtmxVsa.Features.GetUserEmailStore.GetUserEmailStoreHelper;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable NotAccessedPositionalProperty.Global

namespace AspNetMartenHtmxVsa.Features.Subscriptions.RegisterUser;

[AllowAnonymous]
public class RegisterUserController : Controller
{
  private readonly ILogger _logger;

  public RegisterUserController(
    ILogger logger
  )
  {
    _logger = logger;
  }

  [HttpGet]
  [Route("/register-user")]
  public IActionResult RegisterUser()
  {
    return View("~/Features/Subscriptions/RegisterUser/RegisterUser.cshtml");
  }

  private IActionResult GenericErrorResult(
    string? errorMessage = "Beim Speichern ist ein Fehler aufgetreten. Bitte versuchen Sie es erneut."
  )
  {
    ModelState.AddModelError(
      "Generic",
      errorMessage!
    );
    return View("~/Features/Subscriptions/RegisterUser/_RegistrationForm.cshtml");
  }

  [HttpPost]
  [Route("/register-user")]
  public async Task<IActionResult> RegisterUser(
    [FromServices] IUserStore<AppUser> userStore,
    [FromServices] UserManager<AppUser> userManager,
    [FromServices] IGlobalStore globalStore,
    [FromServices] ISubscribersStore subscribersStore,
    [FromServices] EventStoreConfiguration storeConfiguration,
    [FromForm] Registration registration,
    CancellationToken token
  )
  {
    try
    {
      if (!ModelState.IsValid)
        return PartialView(
          "~/Features/Subscriptions/RegisterUser/_RegistrationForm.cshtml",
          registration
        );

      var emailStore = GetEmailStore(userManager, userStore);


      try
      {
        var handler = new RegisterUserHandler(
          userStore,
          userManager,
          globalStore,
          subscribersStore,
          storeConfiguration,
          emailStore
        );

        var result = await handler.Handle(registration, token);

        return result switch
        {
          {
            IsOk: true
          } => PartialView(
            "~/Features/Subscriptions/RegisterUser/_RegistrationSucceeded.cshtml",
            result.Value
          ),
          {
            Error:
            {
            } error
          } => GenericErrorResult(error.Message),
          _ => GenericErrorResult()
        };
      }
      catch (Exception exception)
      {
        var result = exception switch
        {
          UserAlreadyExistsException => HandleUserAlreadyExistsResult(),
          _ => GenericErrorResult()
        };

        return result;
      }
    }
    catch (Exception exception)
    {
      _logger.LogCritical(
        exception,
        "Unexpected Exception during user registration {Message}",
        exception.Message
      );
      return GenericErrorResult();
    }
  }

  private IActionResult HandleUserAlreadyExistsResult()
  {
    ModelState.AddModelError(
      "Email",
      "Benutzer existiert bereits"
    );
    return View("~/Features/Subscriptions/RegisterUser/_RegistrationForm.cshtml");
  }

  [HttpPost]
  [Route("/api/password-validation")]
  public IActionResult PasswordValidation(
    [FromForm] PasswordValidation passwordValidation
  )
  {
    if (string.IsNullOrWhiteSpace(passwordValidation.Password))
      return PartialView(
        "~/Features/Subscriptions/RegisterUser/_PasswordStrength.cshtml",
        new PasswordValidation
        {
          PasswordScore = null
        }
      );

    var validator = new PasswordValidatorService(new PasswordRequirements());
    validator.TestAndScore(passwordValidation.Password);
    return PartialView(
      "~/Features/Subscriptions/RegisterUser/_PasswordStrength.cshtml",
      new PasswordValidation
      {
        PasswordScore = validator.Score
      }
    );
  }
}

public class RegisterUserHandler
{
  private readonly IUserStore<AppUser> _userStore;
  private readonly UserManager<AppUser> _userManager;
  private readonly IGlobalStore _globalStore;
  private readonly ISubscribersStore _subscribersStore;
  private readonly EventStoreConfiguration _storeConfiguration;
  private readonly IUserEmailStore<AppUser> _emailStore;

  public RegisterUserHandler(
    IUserStore<AppUser> userStore,
    UserManager<AppUser> userManager,
    IGlobalStore globalStore,
    ISubscribersStore subscribersStore,
    EventStoreConfiguration storeConfiguration,
    IUserEmailStore<AppUser> emailStore
  )
  {
    _userStore = userStore;
    _userManager = userManager;
    _globalStore = globalStore;
    _subscribersStore = subscribersStore;
    _storeConfiguration = storeConfiguration;
    _emailStore = emailStore;
  }

  public async Task<Result<SubscriptionId?, Error>> Handle(
    Registration registration,
    CancellationToken token
  )
  {
    try
    {
      var companyName = registration.CompanyName!;
      var email = registration.Email!;
      var firstName = registration.FirstName!;
      var lastName = registration.LastName!;

      var tenantId = Guid.NewGuid();
      var tenantIdString = tenantId.ToString();


      var password = registration.Password!;


      var user = new AppUser
      {
        FirstName = firstName,
        LastName = lastName
      };
      await _userStore.SetUserNameAsync(
        user,
        email,
        CancellationToken.None
      );
      await _emailStore.SetEmailAsync(
        user,
        email,
        CancellationToken.None
      );
      var result = await _userManager.CreateAsync(
        user,
        password
      );

      if (result.Succeeded)
      {
        await _userManager.AddClaimAsync(
          user,
          new Claim(
            "tenantId",
            tenantIdString
          )
        );
        var userId = new UserId(Guid.NewGuid());
        var employeeId = new EmployeeId(Guid.NewGuid());
        var subscriptionId = new SubscriptionId(tenantId);
        var organizationId = OrganizationId.FromSubscription(subscriptionId);

        var sub = user.Id;


        var userRegistered = new UserRegistered(
          userId,
          email,
          firstName,
          lastName,
          companyName,
          tenantIdString,
          sub,
          DateTimeOffset.Now
        );
        var registrationConfirmed =
          new RegistrationConfirmed(
            userId,
            email,
            firstName,
            lastName,
            companyName,
            tenantIdString,
            sub,
            DateTimeOffset.Now
          );

        var globalEventStoreId = EventStore.GetDefaultEventStoreId(_storeConfiguration);
        var subscriptionEventStoreId = EventStore.GetSubscriberEventStoreId(subscriptionId);
        var inquiryFormId = Guid.NewGuid();

        // global store -> subscription stream
        var subscribed = new Subscribed(
          companyName,
          email,
          DateTimeOffset.Now,
          subscriptionId
        );


        // tenant store -> organization stream
        var companyNameSetFromUserRegistration = new CompanyNameSetFromUserRegistration(companyName);

        await using var globalSession = _globalStore.LightweightSession(globalEventStoreId);
        await using var subscriptionSession = _subscribersStore.LightweightSession(subscriptionEventStoreId);

        var userEvents = new object[] { userRegistered, registrationConfirmed };
        var subscriptionEvents = new object[] { subscribed };

        var organizationEvents = new object[] { companyNameSetFromUserRegistration };

        globalSession.Events.StartStream(userId.Value, userEvents);
        globalSession.Events.StartStream(subscriptionId.Value, subscriptionEvents);

        await globalSession.SaveChangesAsync(token);

        subscriptionSession.Events.StartStream(organizationId.Value, organizationEvents);

        await subscriptionSession.SaveChangesAsync(token);


        return subscriptionId;
      }
      else
      {
        // ReSharper disable once RedundantAssignment
        // ReSharper disable once EntityNameCapturedOnly.Local
        var errorDescriber = new IdentityErrorDescriber();
        if (result.Errors.Any(error => error.Code == nameof(errorDescriber.DuplicateUserName)))
          throw new UserAlreadyExistsException();

        throw new ApplicationException();
      }
    }
    catch (Exception exception)
    {
      return new Error(exception.Message);
    }
  }
}

public class RegistrationModel
{
  [BindProperty] public Registration? Input { get; set; }
}

public class Registration
{
  public string? FirstName { get; set; }

  public string? LastName { get; set; }

  public string? CompanyName { get; set; }

  public string? Password { get; set; }

  public string? Email { get; set; }

  [Range(
    typeof(bool),
    "true",
    "true",
    ErrorMessage = "Accepting the terms is required."
  )]
  public bool TermsAccepted { get; set; }
}

public class PasswordValidation
{
  public int? PasswordScore { get; set; }
  public string? Password { get; set; }
}

public record UserRegistered(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string CompanyName,
  string TenantId,
  string Sub,
  DateTimeOffset On
);

public record RegistrationConfirmed(
  UserId UserId,
  string Email,
  string FirstName,
  string LastName,
  string CompanyName,
  string TenantId,
  string Sub,
  DateTimeOffset On
);

public class RegisteredUserProjection : SingleStreamProjection<RegisteredUser>
{
  public RegisteredUserProjection()
  {
    DeleteEvent<RegistrationConfirmed>();
  }

  public RegisteredUser Create(
    UserRegistered registered
  )
  {
    var initials = $"{registered.FirstName[..1]}{registered.LastName[..1]}";

    return new RegisteredUser(
      registered.UserId.Value,
      registered.Email,
      registered.FirstName,
      registered.LastName,
      initials,
      registered.Sub,
      Guid.Parse(
        registered.TenantId
      ),
      registered.On
    );
  }
}

public class ActiveUserProjection : SingleStreamProjection<ActiveUser>
{
  public ActiveUser Create(
    RegistrationConfirmed confirmed
  )
  {
    var initials = $"{confirmed.FirstName[..1]}{confirmed.LastName[..1]}";

    return new ActiveUser(
      confirmed.UserId.Value,
      confirmed.Email,
      confirmed.FirstName,
      confirmed.LastName,
      initials,
      confirmed.Sub,
      Guid.Parse(
        confirmed.TenantId
      ),
      confirmed.On
    );
  }
}

public record ActiveUser(
  Guid Id,
  string Email,
  string FirstName,
  string LastName,
  string Initials,
  string Sub,
  Guid SubscriptionId,
  DateTimeOffset RegistrationConfirmedOn,
  string Type = nameof(ActiveUser)
) : User(
  Id,
  Email,
  Email,
  FirstName,
  LastName,
  Initials,
  SubscriptionId,
  Sub
);

public record RegisteredUser(
  Guid Id,
  string Email,
  string FirstName,
  string LastName,
  string Initials,
  string Sub,
  Guid SubscriptionId,
  DateTimeOffset RegisteredOn,
  string Type = nameof(RegisteredUser)
) : User(
  Id,
  Email,
  Email,
  FirstName,
  LastName,
  Initials,
  SubscriptionId,
  Sub
);

public static class UserProjectionConfiguration
{
  public static void UseRegisteredUserProjections(
    this StoreOptions options
  )
  {
    options.Schema
      .For<User>()
      .AddSubClass<RegisteredUser>()
      .AddSubClass<ActiveUser>();

    options.Projections.Add<RegisteredUserProjection>(ProjectionLifecycle.Inline);
    options.Projections.Add<ActiveUserProjection>(ProjectionLifecycle.Inline);
  }
}

public class UserAlreadyExistsException : ApplicationException
{
  public UserAlreadyExistsException() : base("User already exists")
  {
  }
}
