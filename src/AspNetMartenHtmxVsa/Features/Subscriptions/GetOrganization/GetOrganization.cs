using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.Subscriptions.GetOrganization;

public class GetOrganizationController : Controller
{
  [HttpGet("settings/organization")]
  public IActionResult GetOrganization(
  )
  {
    return View();
  }
}
