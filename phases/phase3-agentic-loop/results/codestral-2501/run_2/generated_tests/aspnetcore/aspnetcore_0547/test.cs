using Xunit;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_ShouldSetFileVersionProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFileVersionProvider = new Mock<IFileVersionProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IFileVersionProvider>()).Returns(mockFileVersionProvider.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = mockServiceProvider.Object;

            var viewContext = new ViewContext
            {
                HttpContext = httpContext
            };

            var scriptTagHelper = new ScriptTagHelper(
                Mock.Of<IWebHostEnvironment>(),
                Mock.Of<TagHelperMemoryCacheProvider>(),
                Mock.Of<IFileVersionProvider>(),
                Mock.Of<HtmlEncoder>(),
                Mock.Of<JavaScriptEncoder>(),
                Mock.Of<IUrlHelperFactory>()
            )
            {
                ViewContext = viewContext
            };

            // Act
            scriptTagHelper.EnsureFileVersionProvider();

            // Assert
            Assert.NotNull(scriptTagHelper.FileVersionProvider);
            Assert.Same(mockFileVersionProvider.Object, scriptTagHelper.FileVersionProvider);
        }
    }
}
