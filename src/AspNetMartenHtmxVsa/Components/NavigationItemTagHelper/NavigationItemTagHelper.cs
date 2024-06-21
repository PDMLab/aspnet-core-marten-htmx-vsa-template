using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Tailwind.Heroicons;

namespace AspNetMartenHtmxVsa.Components.NavigationItemTagHelper;

public class NavigationItem
{
  public NavigationItem(
    string text,
    string href,
    string? uriTemplate,
    string[] hrefs,
    IconSymbol? icon = null
  )
  {
    Text = text;
    Href = href;
    UriTemplate = uriTemplate;
    Hrefs = hrefs;
    Icon = icon;
  }

  public string Text { get; set; }
  public string Href { get; set; }
  public string? UriTemplate { get; set; }
  public string[] Hrefs { get; set; }
  public IconSymbol? Icon { get; set; }

  public bool IsMatch(
    string url
  )
  {
    if (UriTemplate is null) return Hrefs.Contains(url);

    var regexPattern = Regex.Escape(UriTemplate)
      .Replace("\\{", "(?<")
      .Replace("}", ">[^\\/\\?]+)")
      .Replace("/", "\\/")
      .Replace("\\?", "\\?") + "$";

    var regex = new Regex(regexPattern);
    return regex.IsMatch(url);
  }
}

public class NavigationItemTagHelper : PartialTagHelperBase.PartialTagHelperBase
{
  public NavigationItemTagHelper(
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
    if (Item is null) throw new ArgumentNullException(nameof(Item));
    var content = await RenderPartial("NavigationItemTagHelper", Item);
    output.PreContent.AppendHtml(content);
  }
}
