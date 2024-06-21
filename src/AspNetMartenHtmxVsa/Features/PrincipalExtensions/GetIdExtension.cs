using System.Security.Claims;

namespace AspNetMartenHtmxVsa.Features.PrincipalExtensions;

public static class GetIdClaimsExtensions
{
  public static string? GetSub(
    this ClaimsPrincipal user
  )
  {
    return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
      ?.Value;
  }

  public static Guid GetSubAsGuid(
    this ClaimsPrincipal user
  )
  {
    return Guid.Parse(user.GetSub()!);
  }
}
