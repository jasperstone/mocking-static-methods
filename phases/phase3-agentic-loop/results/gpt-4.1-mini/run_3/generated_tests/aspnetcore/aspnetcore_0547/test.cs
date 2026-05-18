using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_CallsGetRequiredServiceOnRequestServices()
        {
            // Arrange
            var mockFileVersionProvider = new Mock<IFileVersionProvider>();
            var mockHostingEnvironment = new Mock<IWebHostEnvironment>();
            var mockCacheProvider = new Mock<TagHelperMemoryCacheProvider>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            mockCacheProvider.Setup(p => p.Cache).Returns(memoryCache);
            var htmlEncoder = HtmlEncoder.Default;
            var javaScriptEncoder = JavaScriptEncoder.Default;
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();

            var mockRequestServices = new Mock<IServiceProvider>();
            mockRequestServices
                .Setup(s => s.GetRequiredService(typeof(IFileVersionProvider)))
                .Returns(mockFileVersionProvider.Object)
                .Verifiable();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockRequestServices.Object);

            var viewContext = new ViewContext();
            viewContext.HttpContext = mockHttpContext.Object;

            var scriptTagHelper = new ScriptTagHelper(
                mockHostingEnvironment.Object,
                mockCacheProvider.Object,
                null, // We want to test the call to GetRequiredService, so start with null
                htmlEncoder,
                javaScriptEncoder,
                mockUrlHelperFactory.Object)
            {
                ViewContext = viewContext
            };

            // Act
            // Call the private method EnsureFileVersionProvider via reflection to test the call to GetRequiredService
            var methodInfo = typeof(ScriptTagHelper).GetMethod("EnsureFileVersionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);
            methodInfo.Invoke(scriptTagHelper, null);

            // Assert
            mockRequestServices.Verify(s => s.GetRequiredService(typeof(IFileVersionProvider)), Times.Once());
        }
    }
}
