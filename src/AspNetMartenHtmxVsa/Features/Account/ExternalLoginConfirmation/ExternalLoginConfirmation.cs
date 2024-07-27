using System.ComponentModel.DataAnnotations;

namespace AspNetMartenHtmxVsa.Features.Account.ExternalLoginConfirmation;

public class ExternalLoginConfirmationViewModel
{
  [Required] [EmailAddress] public string Email { get; set; }
}
