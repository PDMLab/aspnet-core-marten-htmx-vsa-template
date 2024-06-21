using JetBrains.Annotations;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetMartenHtmxVsa.Components.PartialTagHelperBase;

public class PartialTagHelperBase : TagHelper
{
  // ReSharper disable once UnusedAutoPropertyAccessor.Global
  [HtmlAttributeNotBound] [ViewContext] public ViewContext? ViewContext { get; set; }
  private IHtmlHelper _htmlHelper;

  public PartialTagHelperBase(
    IHtmlHelper htmlHelper
  ) =>
    _htmlHelper = htmlHelper;

  protected async Task<IHtmlContent> RenderPartial<T>(
    [AspMvcPartialView] string partialName,
    T model
  )
  {
    (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);

    return await _htmlHelper.PartialAsync(partialName, model);
  }
}
