using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_CallsGetRequiredService()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var fileVersionProviderMock = new Mock<IFileVersionProvider>();

            serviceProviderMock
                .Setup(s => s.GetRequiredService<IFileVersionProvider>())
                .Returns(fileVersionProviderMock.Object);

            var tagHelper = new ScriptTagHelper(
                new Mock<IWebHostEnvironment>().Object,
                new TagHelperMemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())),
                null, // FileVersionProvider is null initially
                Mock.Of<HtmlEncoder>(),
                Mock.Of<JavaScriptEncoder>(),
                Mock.Of<IUrlHelperFactory>());

            tagHelper.ViewContext = new ViewContext
            {
                HttpContext = httpContext
            };

            httpContext.RequestServices = serviceProviderMock.Object;

            // Act
            tagHelper.EnsureFileVersionProvider();

            // Assert
            serviceProviderMock.Verify(s => s.GetRequiredService<IFileVersionProvider>(), Times.Once);
            Assert.Same(fileVersionProviderMock.Object, tagHelper.FileVersionProvider);
        }
    }
}
