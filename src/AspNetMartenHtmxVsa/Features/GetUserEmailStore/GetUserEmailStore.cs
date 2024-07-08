using AspNetMartenHtmxVsa.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace AspNetMartenHtmxVsa.Features.GetUserEmailStore;

public static class GetUserEmailStoreHelper
{
  public static IUserEmailStore<AppUser> GetEmailStore(
    UserManager<AppUser> userManager,
    IUserStore<AppUser> userStore
  )
  {
    if (!userManager.SupportsUserEmail)
    {
      throw new NotSupportedException(
        "The default UI requires a user store with email support."
      );
    }

    return (IUserEmailStore<AppUser>)userStore;
  }
}
