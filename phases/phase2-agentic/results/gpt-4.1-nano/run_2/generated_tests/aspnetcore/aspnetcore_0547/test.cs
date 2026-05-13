using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Encodings.Web;

namespace TagHelperTests
{
    public class ScriptTagHelperTests
    {
        private class DummyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IFileVersionProvider))
                {
                    return new DummyFileVersionProvider();
                }
                return null;
            }
        }

        private class DummyFileVersionProvider : IFileVersionProvider
        {
            public string AddFileVersion(string path)
            {
                return path + "?v=123";
            }
        }

        [Fact]
        public void EnsureFileVersionProvider_Should_Call_GetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddTransient<IFileVersionProvider, DummyFileVersionProvider>();
            var serviceProvider = services.BuildServiceProvider();

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProvider;

            var viewContext = new ViewContext
            {
                HttpContext = httpContext
            };

            var tagHelperContext = new TagHelperContext(
                tagName: "script",
                allAttributes: new TagHelperAttributeList(),
                items: new System.Collections.Generic.Dictionary<object, object>(),
                uniqueId: "test");

            var tagHelperOutput = new TagHelperOutput("script", new TagHelperAttributeList());

            var helper = new ScriptTagHelper(
                new HostingEnvironment(),
                new TagHelperMemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                new DummyFileVersionProvider(),
                HtmlEncoder.Default,
                JavaScriptEncoder.Default,
                new DefaultUrlHelperFactory())
            {
                ViewContext = viewContext
            };

            // Act
            helper.ViewContext = viewContext;
            helper.GetType().GetMethod("EnsureFileVersionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(helper, null);

            // Assert
            var fileVersionProvider = helper.FileVersionProvider;
            Assert.NotNull(fileVersionProvider);
            Assert.IsType<DummyFileVersionProvider>(fileVersionProvider);
        }
    }
}
