using Marten;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMartenHtmxVsa.Features
{
  public partial class ApiRouteNames
  {
    public const string RebuildProjections = nameof(RebuildProjections);
    public const string GetProjections = nameof(GetProjections);
  }
}

namespace AspNetMartenHtmxVsa.Features.RebuildProjections
{
  public class RebuildProjectionsController : Controller
  {
    [HttpGet("/api/projections", Name = ApiRouteNames.GetProjections)]
    public IActionResult GetProjections(
      [FromServices] IDocumentStore store
    )
    {
      var documentTypes = store.Options.Events.Projections()
        .OrderBy(p => p.ProjectionName)
        .AsEnumerable();
      return View(documentTypes);
    }

    [HttpPost("/api/projections", Name = ApiRouteNames.RebuildProjections)]
    public async Task<IActionResult> RebuildProjections(
      [FromQuery] string alias,
      [FromServices] IDocumentStore store,
      CancellationToken cancellationToken
    )
    {
      using var daemon = await store.BuildProjectionDaemonAsync();
      await daemon.StartAllAsync();
      await daemon.RebuildProjectionAsync(alias, cancellationToken);
      await daemon.StopAllAsync();
      return Content($"Projection {alias} has been rebuild.");
    }
  }
}
