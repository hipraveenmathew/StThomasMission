using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StThomasMission.Web.TagHelpers
{
    [HtmlTargetElement("th", Attributes = "sort-by")]
    public class SortHeaderTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public SortHeaderTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        [HtmlAttributeName("sort-by")]
        public string SortBy { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var currentSort = ViewContext.ViewData["CurrentSort"] as string;

            output.TagName = "th";

            string newSortOrder = (currentSort == SortBy) ? $"{SortBy}_desc" : SortBy;

            var routeValues = new RouteValueDictionary();
            // Preserve existing query string values
            foreach (var key in ViewContext.HttpContext.Request.Query.Keys)
            {
                routeValues[key] = ViewContext.HttpContext.Request.Query[key];
            }
            routeValues["sortOrder"] = newSortOrder;

            var action = ViewContext.RouteData.Values["action"]!.ToString();
            var url = urlHelper.Action(action, routeValues);

            output.Content.AppendHtml($"<a href=\"{url}\" class=\"text-decoration-none\">");
            output.Content.Append(output.GetChildContentAsync().Result.GetContent());

            if (currentSort == SortBy)
            {
                output.Content.AppendHtml(" <i class=\"fas fa-sort-up\"></i>");
            }
            else if (currentSort == $"{SortBy}_desc")
            {
                output.Content.AppendHtml(" <i class=\"fas fa-sort-down\"></i>");
            }
            else
            {
                output.Content.AppendHtml(" <i class=\"fas fa-sort text-muted\"></i>");
            }

            output.Content.AppendHtml("</a>");
        }
    }
}