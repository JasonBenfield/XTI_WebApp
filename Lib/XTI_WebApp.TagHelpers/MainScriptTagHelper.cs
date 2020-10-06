using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using XTI_WebApp.Extensions;

namespace XTI_WebApp.TagHelpers
{
    [HtmlTargetElement("xti-main-script", TagStructure = TagStructure.WithoutEndTag)]
    public class MainScriptTagHelper : TagHelper
    {
        public MainScriptTagHelper(IWebHostEnvironment host, IUrlHelperFactory urlHelperFactory, CacheBust cacheBust)
        {
            this.host = host;
            this.urlHelperFactory = urlHelperFactory;
            this.cacheBust = cacheBust;
        }

        private readonly IWebHostEnvironment host;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly CacheBust cacheBust;

        public string PageName { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "script";
            string path;
            if (host.IsDevelopment() || host.IsEnvironment("Test"))
            {
                path = "~/js/dev/";
            }
            else
            {
                path = "~/js/dist/";
            }
            var urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            var pageUrl = await getPageUrl(urlHelper, path);
            output.Attributes.Add("src", pageUrl);
            output.TagMode = TagMode.StartTagAndEndTag;
        }

        private async Task<string> getPageUrl(IUrlHelper urlHelper, string path)
        {
            var query = await cacheBust.Query();
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = $"?{query}";
            }
            return urlHelper.Content($"{path}{PageName}.js{query}");
        }
    }
}
