using System.Security.Claims;
using AspNetMartenHtmxVsa.Core;

namespace AspNetMartenHtmxVsa.Features.PrincipalExtensions;

public static class ClaimPrincipalExtensions
{
  public static string GetInitials(
    this ClaimsPrincipal user
  )
  {
    var firstName =
      user.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.GivenName)
        ?.Value;
    var lastName =
      user.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.Surname)
        ?.Value;

    return $"{firstName?[..1]}{lastName?[..1]}";
  }

  public static SubscriptionId GetSubscriptionId(
    this ClaimsPrincipal principal
  )
  {
    return new SubscriptionId(
      Guid.Parse(
        principal
          .GetTenantIdClaim()
          .Value
      )
    );
  }

  public static Claim GetTenantIdClaim(
    this ClaimsPrincipal principal
  )
  {
    return principal.Claims
      .First(c => c.Type == Constants.TenantIdClaimName);
  }

  public static string GetId(
    this ClaimsPrincipal principal
  )
  {
    return principal.Claims
      .First(c => c.Type == ClaimTypes.NameIdentifier)
      .Value;
  }
}
