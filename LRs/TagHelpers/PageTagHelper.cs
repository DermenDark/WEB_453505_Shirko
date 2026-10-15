
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace LRs.TagHelpers;

[HtmlTargetElement("pager")]
public sealed class PagerTagHelper : TagHelper
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PagerTagHelper(
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    [HtmlAttributeName("current-page")]
    public int CurrentPage { get; set; } = 1;

    [HtmlAttributeName("total-pages")]
    public int TotalPages { get; set; }

    [HtmlAttributeName("category")]
    public string? Category { get; set; }

    [HtmlAttributeName("admin")]
    public bool Admin { get; set; }

    public override void Process(
        TagHelperContext context,
        TagHelperOutput output)
    {

        if (TotalPages <= 1)
        {
            output.SuppressOutput();
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            output.SuppressOutput();
            return;
        }

        int currentPage = Math.Clamp(CurrentPage, 1, TotalPages);

        var nav = new TagBuilder("nav");
        nav.AddCssClass("mt-4");
        nav.Attributes["aria-label"] = "Навигация по страницам";

        var ul = new TagBuilder("ul");
        ul.AddCssClass("pagination");

        ul.InnerHtml.AppendHtml(
            CreatePageItem(
                httpContext,
                "Предыдущая",
                currentPage - 1,
                disabled: currentPage == 1));

        for (int page = 1; page <= TotalPages; page++)
        {
            ul.InnerHtml.AppendHtml(
                CreatePageItem(
                    httpContext,
                    page.ToString(),
                    page,
                    active: page == currentPage));
        }

        ul.InnerHtml.AppendHtml(
            CreatePageItem(
                httpContext,
                "Следующая",
                currentPage + 1,
                disabled: currentPage == TotalPages));

        nav.InnerHtml.AppendHtml(ul);

        output.TagName = null;
        output.Content.SetHtmlContent(nav);
    }

    private TagBuilder CreatePageItem(
        HttpContext httpContext,
        string text,
        int targetPage,
        bool active = false,
        bool disabled = false)
    {
        var li = new TagBuilder("li");
        li.AddCssClass("page-item");

        if (active)
        {
            li.AddCssClass("active");
        }

        string? url = disabled
            ? null
            : GenerateUrl(httpContext, targetPage);

        if (disabled || string.IsNullOrWhiteSpace(url))
        {
            li.AddCssClass("disabled");

            var span = new TagBuilder("span");
            span.AddCssClass("page-link");
            span.InnerHtml.Append(text);

            if (active)
            {
                span.Attributes["aria-current"] = "page";
            }

            li.InnerHtml.AppendHtml(span);
            return li;
        }

        var a = new TagBuilder("a");
        a.AddCssClass("page-link");
        a.Attributes["href"] = url;
        a.InnerHtml.Append(text);

        if (active)
        {
            a.Attributes["aria-current"] = "page";
        }

        li.InnerHtml.AppendHtml(a);

        return li;
    }

    private string? GenerateUrl(HttpContext httpContext, int page)
    {
        if (Admin)
        {
            return _linkGenerator.GetPathByPage(
                httpContext,
                page: "/Bikes/Index",
                values: new
                {
                    pageNo = page
                });
        }

        return _linkGenerator.GetPathByAction(
            httpContext,
            action: "Index",
            controller: "Product",
            values: new
            {
                categoryNormalizedName = Category,
                pageNo = page
            });
    }
}
