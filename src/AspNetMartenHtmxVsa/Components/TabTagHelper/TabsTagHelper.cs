using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetMartenHtmxVsa.Components.TabTagHelper;

public class TabsTagHelper : TagHelper
{
  private readonly IHtmlHelper _html;

  [HtmlAttributeNotBound] [ViewContext] public ViewContext? ViewContext { get; set; }

  public TabsTagHelper(
    IHtmlHelper htmlHelper
  )
  {
    _html = htmlHelper;
  }

  public override async Task ProcessAsync(
    TagHelperContext context,
    TagHelperOutput output
  )
  {
    output.TagMode = TagMode.StartTagAndEndTag;
    output.SuppressOutput();
    var childContent = await output.GetChildContentAsync();
    var children = childContent.GetContent();
    (_html as IViewContextAware)?.Contextualize(ViewContext);

    var content = await _html.PartialAsync("~/Components/TabTagHelper/TabsTagHelper.cshtml", children);
    output.PreContent.SetHtmlContent(content);
  }
}
