using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StThomasMission.Core.Interfaces;
using System.Text;

namespace StThomasMission.Web.TagHelpers
{
    [HtmlTargetElement("pagination-controls")]
    public class PaginationControlsTagHelper : TagHelper
    {
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        public IPaginationInfo For { get; set; } = null!;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For == null || For.TotalPages <= 1)
            {
                output.SuppressOutput();
                return;
            }

            output.TagName = "nav";
            output.Attributes.SetAttribute("aria-label", "Page navigation");

            var sb = new StringBuilder();
            sb.Append("<ul class=\"pagination justify-content-center\">");

            // Previous Button
            sb.Append(CreatePageLink(For.PageIndex - 1, For.HasPreviousPage, "<i class=\"fas fa-chevron-left\"></i> Previous"));

            // Page Number Buttons
            for (int i = 1; i <= For.TotalPages; i++)
            {
                sb.Append(CreatePageLink(i, true, i.ToString(), i == For.PageIndex));
            }

            // Next Button
            sb.Append(CreatePageLink(For.PageIndex + 1, For.HasNextPage, "Next <i class=\"fas fa-chevron-right\"></i>"));

            sb.Append("</ul>");
            output.Content.SetHtmlContent(sb.ToString());
        }

        private string CreatePageLink(int pageIndex, bool isEnabled, string text, bool isActive = false)
        {
            var liClass = isEnabled ? "page-item" : "page-item disabled";
            if (isActive) liClass += " active";

            var routeValues = new RouteValueDictionary();
            foreach (var key in ViewContext.HttpContext.Request.Query.Keys)
            {
                if (key.ToLower() != "pagenumber")
                    routeValues[key] = ViewContext.HttpContext.Request.Query[key];
            }
            routeValues["pageNumber"] = pageIndex;

            var url = new Microsoft.AspNetCore.Mvc.Routing.UrlHelper(ViewContext).Action(ViewContext.RouteData.Values["action"]!.ToString(), routeValues);

            return $"<li class=\"{liClass}\"><a class=\"page-link\" href=\"{url}\">{text}</a></li>";
        }
    }
}