using System.Security.Claims;

namespace AspNetMartenHtmxVsa.Features.PrincipalExtensions;

public static class GetFirstnameLastnameClaimsPrincipalExtensions
{
  public static string? GetFirstname(
    this ClaimsPrincipal user
  ) => user.Claims
    .FirstOrDefault(c => c.Type == ClaimTypes.GivenName)
    ?.Value;

  public static string? GetLastname(
    this ClaimsPrincipal user
  ) => user.Claims
    .FirstOrDefault(c => c.Type == ClaimTypes.Surname)
    ?.Value;

  public static string GetFullname(
    this ClaimsPrincipal user
  ) => $"{user.GetFirstname()} {user.GetLastname()}";
}
