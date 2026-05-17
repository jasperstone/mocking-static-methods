using System;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_GetsRequiredServiceFromRequestServices()
        {
            // Arrange
            var mockHostingEnvironment = new Mock<IWebHostEnvironment>();
            var mockCacheProvider = new Mock<TagHelperMemoryCacheProvider>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            mockCacheProvider.Setup(c => c.Cache).Returns(memoryCache);
            var htmlEncoder = HtmlEncoder.Default;
            var javaScriptEncoder = JavaScriptEncoder.Default;
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();

            // Pass null for IFileVersionProvider to test lazy loading
            var scriptTagHelper = new ScriptTagHelper(
                mockHostingEnvironment.Object,
                mockCacheProvider.Object,
                null,
                htmlEncoder,
                javaScriptEncoder,
                mockUrlHelperFactory.Object);

            var mockFileVersionProviderFromService = new Mock<IFileVersionProvider>();

            var mockRequestServices = new Mock<IServiceProvider>();
            mockRequestServices
                .Setup(s => s.GetService(typeof(IFileVersionProvider)))
                .Returns(mockFileVersionProviderFromService.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockRequestServices.Object);

            var viewContext = new ViewContext();
            viewContext.HttpContext = mockHttpContext.Object;

            scriptTagHelper.ViewContext = viewContext;

            // Act
            // Use reflection to invoke private method EnsureFileVersionProvider
            var method = typeof(ScriptTagHelper).GetMethod("EnsureFileVersionProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(scriptTagHelper, null);

            // Assert
            // Use reflection to get the private field FileVersionProvider
            var field = typeof(ScriptTagHelper).GetField("FileVersionProvider", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileVersionProviderValue = field.GetValue(scriptTagHelper);

            Assert.NotNull(fileVersionProviderValue);
            Assert.Same(mockFileVersionProviderFromService.Object, fileVersionProviderValue);
        }
    }
}
