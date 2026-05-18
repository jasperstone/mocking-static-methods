using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_WhenProviderIsNull_UsesGetRequiredService()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var viewContext = new ViewContext
            {
                HttpContext = httpContext,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            };

            var serviceProviderMock = new Mock<IServiceProvider>();
            var fileVersionProviderMock = new Mock<IFileVersionProvider>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IFileVersionProvider>())
                .Returns(fileVersionProviderMock.Object);

            var hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
            var cacheProvider = new TagHelperMemoryCacheProvider(new MemoryCache(new MemoryCacheOptions()));
            var htmlEncoder = Mock.Of<HtmlEncoder>();
            var javaScriptEncoder = Mock.Of<JavaScriptEncoder>();
            var urlHelperFactory = Mock.Of<IUrlHelperFactory>();

            var scriptTagHelper = new ScriptTagHelper(
                hostingEnvironmentMock.Object,
                cacheProvider,
                null, // FileVersionProvider is null initially
                htmlEncoder,
                javaScriptEncoder,
                urlHelperFactory);

            scriptTagHelper.ViewContext = viewContext;

            // Act
            scriptTagHelper.EnsureFileVersionProvider();

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IFileVersionProvider>(), Times.Once);
            Assert.Same(fileVersionProviderMock.Object, scriptTagHelper.FileVersionProvider);
        }
    }
}
