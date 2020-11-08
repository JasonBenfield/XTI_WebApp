using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_App;
using XTI_WebApp.Extensions;
using XTI_WebApp.Fakes;
using XTI_WebApp.TagHelpers;
using XTI_WebApp.TestFakes;

namespace XTI_WebApp.AspTests
{
    public class MainScriptTagHelperTest
    {
        [Test]
        public async Task ShouldOutputScript()
        {
            var input = await setup();
            var result = await execute(input);
            Assert.That(result.TagName, Is.EqualTo("script"));
        }

        [Test]
        public async Task ShouldAddSrcAttribute()
        {
            var input = await setup();
            var result = await execute(input);
            Assert.That(result.Attributes.Count, Is.EqualTo(1));
            Assert.That(result.Attributes[0].Name, Is.EqualTo("src"));
        }

        [Test]
        public async Task ShouldUsePageNameInSrc()
        {
            var input = await setup();
            input.TagHelper.PageName = "home";
            var result = await execute(input);
            var src = result.Attributes[0].Value;
            Assert.That(src, Does.Contain($"/home.js?"));
        }

        [Test]
        public async Task ShouldIncludeCacheBustFromWebAppOptions()
        {
            var input = await setup();
            input.WebAppOptions.Value.CacheBust = "X";
            var result = await execute(input);
            var src = result.Attributes[0].Value;
            Assert.That(src, Does.EndWith("?cacheBust=X"));
        }

        [Test]
        public async Task ShouldIncludeVersionIfPathIsCurrent()
        {
            var input = await setup("/Fake/Current/Group/Action");
            input.Environment.EnvironmentName = "Production";
            var result = await execute(input);
            var src = result.Attributes[0].Value;
            Assert.That(src, Does.EndWith($"?cacheBust=V{input.CurrentVersion.ID}"));
        }

        [Test]
        public async Task ShouldNotIncludeVersionIfPathHasExplicitVersion()
        {
            var input = await setup("/Fake/V1/Group/Action");
            input.Environment.EnvironmentName = "Production";
            var result = await execute(input);
            var src = result.Attributes[0].Value;
            Assert.That(src, Does.EndWith($".js"));
        }

        [Test]
        [TestCase("Development", "dev")]
        [TestCase("Test", "dev")]
        [TestCase("Staging", "dist")]
        [TestCase("Production", "dist")]
        public async Task ShouldChangePathBasedOnEnvironment(string envName, string expectedPath)
        {
            var input = await setup();
            input.Environment.EnvironmentName = envName;
            var result = await execute(input);
            var src = result.Attributes[0].Value;
            Assert.That(src, Does.StartWith($"/js/{expectedPath}/"));
        }

        private async Task<TestInput> setup(string path = "/Fake/Current/Group/Action")
        {
            var services = new ServiceCollection();
            services.AddFakesForXtiWebApp();
            services.AddFakeXtiContexts();
            services.AddSingleton(sp => FakeAppKey.AppKey);
            services.AddSingleton(sp => (IWebHostEnvironment)sp.GetService<IHostEnvironment>());
            services.AddSingleton<IOptions<WebAppOptions>, FakeOptions<WebAppOptions>>();
            services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();
            services.AddSingleton<CacheBust>();
            services.AddSingleton(sp => XtiPath.Parse(path));
            services.AddSingleton<MainScriptTagHelper>();
            services.AddScoped<FakeAppSetup>();
            var sp = services.BuildServiceProvider();
            var setup = sp.GetService<FakeAppSetup>();
            await setup.Run();
            return new TestInput(sp);
        }

        private async Task<TagHelperOutput> execute(TestInput input)
        {
            var tagHelperContext = new TagHelperContext
            (
                new TagHelperAttributeList(),
                new Dictionary<object, object>(),
                Guid.NewGuid().ToString("N")
            );
            var tagHelperOutput = new TagHelperOutput
            (
                "xti-main-script",
                new TagHelperAttributeList(),
                (result, encoder) =>
                {
                    var tagHelperContent = new DefaultTagHelperContent();
                    tagHelperContent.SetHtmlContent(string.Empty);
                    return Task.FromResult<TagHelperContent>(tagHelperContent);
                }
            );
            await input.TagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);
            return tagHelperOutput;
        }

        private sealed class TestInput
        {
            public TestInput(ServiceProvider sp)
            {
                Environment = (FakeWebHostEnvironment)sp.GetService<IHostEnvironment>();
                Environment.EnvironmentName = "Test";
                TagHelper = sp.GetService<MainScriptTagHelper>();
                TagHelper.PageName = "home";
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                httpContextAccessor.HttpContext = new DefaultHttpContext
                {
                    RequestServices = sp
                };
                TagHelper.ViewContext = new Microsoft.AspNetCore.Mvc.Rendering.ViewContext()
                {
                    ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor
                    {
                        RouteValues = new Dictionary<string, string>()
                    },
                    HttpContext = httpContextAccessor.HttpContext,
                    RouteData = new Microsoft.AspNetCore.Routing.RouteData()
                };
                WebAppOptions = (FakeOptions<WebAppOptions>)sp.GetService<IOptions<WebAppOptions>>();
                WebAppOptions.Value.CacheBust = "";
                var setup = sp.GetService<FakeAppSetup>();
                App = setup.App;
                CurrentVersion = setup.CurrentVersion;
            }
            public FakeWebHostEnvironment Environment { get; }
            public FakeOptions<WebAppOptions> WebAppOptions { get; }
            public MainScriptTagHelper TagHelper { get; }
            public App App { get; }
            public AppVersion CurrentVersion { get; }
        }
    }
}
