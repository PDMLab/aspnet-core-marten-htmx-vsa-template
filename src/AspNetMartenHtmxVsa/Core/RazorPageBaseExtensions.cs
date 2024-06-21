using Htmx;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Razor;

namespace AspNetMartenHtmxVsa.Core;

public static class RazorPageBaseExtensions
{
  public static string? NonHtmxLayout<T>(
    this RazorPage<T> obj,
    [AspMvcPartialView] string? layout
  )
  {
    return obj.Context.Request.IsHtmxNonBoosted()
      ? null
      : layout;
  }
}
