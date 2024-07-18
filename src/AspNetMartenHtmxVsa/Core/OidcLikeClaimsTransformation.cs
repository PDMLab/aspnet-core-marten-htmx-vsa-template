using System.Security.Claims;
using AspNetMartenHtmxVsa.Features.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace AspNetMartenHtmxVsa.Core;

public class OidcLikeClaimsTransformation : IClaimsTransformation
{
  private readonly UserManager<AppUser> _userManager;

  public OidcLikeClaimsTransformation(
    UserManager<AppUser> userManager
  )
  {
    _userManager = userManager;
  }

  public async Task<ClaimsPrincipal> TransformAsync(
    ClaimsPrincipal principal
  )
  {
    var claimsIdentity = new ClaimsIdentity();
    var user = await _userManager.GetUserAsync(principal);

    if (user is null) throw new ArgumentNullException(nameof(user));
    if (string.IsNullOrWhiteSpace(user.FirstName)) throw new ArgumentNullException(nameof(user.FirstName));
    if (string.IsNullOrWhiteSpace(user.LastName)) throw new ArgumentNullException(nameof(user.LastName));

    const string subClaimType = "sub";
    if (!principal.HasClaim(claim => claim.Type == subClaimType))
    {
      var userId = _userManager.GetUserId(principal);

      if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));

      claimsIdentity.AddClaim(
        new Claim(
          subClaimType,
          userId
        )
      );
    }

    if (!principal.HasClaim(claim => claim.Type == ClaimTypes.GivenName))
      claimsIdentity.AddClaim(
        new Claim(
          ClaimTypes.GivenName,
          user.FirstName
        )
      );

    if (!principal.HasClaim(claim => claim.Type == ClaimTypes.Surname))
      claimsIdentity.AddClaim(
        new Claim(
          ClaimTypes.Surname,
          user.LastName
        )
      );

    principal.AddIdentity(claimsIdentity);
    return principal;
  }
}
