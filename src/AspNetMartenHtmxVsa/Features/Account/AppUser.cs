using Microsoft.AspNetCore.Identity;

namespace AspNetMartenHtmxVsa.Features.Account;

// Add profile data for application users by adding properties to the AppUser class
public class AppUser : IdentityUser
{
  public string? FirstName { get; set; }
  public string? LastName { get; set; }
}
