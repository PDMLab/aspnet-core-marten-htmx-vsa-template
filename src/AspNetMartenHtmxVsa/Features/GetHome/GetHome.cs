using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features.GetHome;

[Authorize]
public class GetHomeController(ILogger<GetHomeController> logger) : Controller
{
  private readonly ILogger<GetHomeController> _logger = logger;

  [HttpGet("/")]
  public IActionResult GetHome() => View();
}
