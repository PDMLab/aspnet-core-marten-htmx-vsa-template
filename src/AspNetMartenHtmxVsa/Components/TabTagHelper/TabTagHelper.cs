using AspNetMartenHtmxVsa.Components.NavigationItemTagHelper;
using AspNetMartenHtmxVsa.Components.PartialTagHelperBase;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetMartenHtmxVsa.Components.TabTagHelper;

public class TabTagHelper : PartialTagHelperBase.PartialTagHelperBase
{
  public TabTagHelper(
    IHtmlHelper htmlHelper
  ) : base(htmlHelper)
  {
  }

  [HtmlAttributeName("item")] public NavigationItem? Item { get; set; }

  public override async Task ProcessAsync(
    TagHelperContext context,
    TagHelperOutput output
  )
  {
    if (Item == null)
      throw new ArgumentNullException(nameof(Item));

    var someContent = await RenderPartial("TabTagHelper", Item);

    output.PreContent.AppendHtml(someContent);
  }
}
