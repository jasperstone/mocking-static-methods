using System;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers;

public class ScriptTagHelperTests
{
    private class FakeFileVersionProvider : IFileVersionProvider
    {
        public string GetVersion(string filePath) => "test";
    }

    [Fact]
    public void EnsureFileVersionProvider_CallsGetRequiredService_WhenNull()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(sp => sp.GetRequiredService<IFileVersionProvider>())
                      .Returns(new FakeFileVersionProvider()).Verifiable();

        var httpContext = new DefaultHttpContext(new Mock<HttpRequest>().Object, new Mock<HttpResponse>().Object, new Mock<IRequestCookieCollection>().Object, new Mock<IResponseCookies>().Object, new Mock<IQueryCollection>().Object, new Mock<IFormCollection>().Object, new Mock<IFormFileCollection>().Object, new Mock<IFormFileCollection>().Object, new Mock<IHeaderDictionary>().Object, new Mock<IHeaderDictionary>().Object, new Mock<IServiceProvider>().Object)
        {
            RequestServices = serviceProvider.Object
        };

        var viewContext = new ViewContext
        {
            HttpContext = httpContext
        };

        var hostingEnvironment = Mock.Of<IWebHostEnvironment>();
        var cacheProvider = Mock.Of<TagHelperMemoryCacheProvider>();
        var htmlEncoder = HtmlEncoder.Default;
        var javaScriptEncoder = JavaScriptEncoder.Default;
        var urlHelperFactory = Mock.Of<IUrlHelperFactory>();

        var tagHelper = new ScriptTagHelper(
            hostingEnvironment,
            cacheProvider,
            null, // FileVersionProvider starts null
            htmlEncoder,
            javaScriptEncoder,
            urlHelperFactory);

        // Use reflection to set the protected ViewContext property from base class
        var viewContextField = typeof(UrlResolutionTagHelper).GetField("_viewContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        viewContextField?.SetValue(tagHelper, viewContext);

        // Act
        tagHelper.EnsureFileVersionProvider();

        // Assert
        serviceProvider.Verify(sp => sp.GetRequiredService<IFileVersionProvider>(), Times.Once);
        Assert.NotNull(tagHelper.FileVersionProvider);
    }

    [Fact]
    public void EnsureFileVersionProvider_DoesNotCallGetRequiredService_WhenAlreadySet()
    {
        // Arrange
        var serviceProvider = new Mock<IServiceProvider>();
        var existingProvider = new FakeFileVersionProvider();

        var httpContext = new DefaultHttpContext(new Mock<HttpRequest>().Object, new Mock<HttpResponse>().Object, new Mock<IRequestCookieCollection>().Object, new Mock<IResponseCookies>().Object, new Mock<IQueryCollection>().Object, new Mock<IFormCollection>().Object, new Mock<IFormFileCollection>().Object, new Mock<IFormFileCollection>().Object, new Mock<IHeaderDictionary>().Object, new Mock<IHeaderDictionary>().Object, new Mock<IServiceProvider>().Object)
        {
            RequestServices = serviceProvider.Object
        };

        var viewContext = new ViewContext
        {
            HttpContext = httpContext
        };

        var hostingEnvironment = Mock.Of<IWebHostEnvironment>();
        var cacheProvider = Mock.Of<TagHelperMemoryCacheProvider>();
        var htmlEncoder = HtmlEncoder.Default;
        var javaScriptEncoder = JavaScriptEncoder.Default;
        var urlHelperFactory = Mock.Of<IUrlHelperFactory>();

        var tagHelper = new ScriptTagHelper(
            hostingEnvironment,
            cacheProvider,
            existingProvider, // FileVersionProvider already set
            htmlEncoder,
            javaScriptEncoder,
            urlHelperFactory);

        var viewContextField = typeof(UrlResolutionTagHelper).GetField("_viewContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        viewContextField?.SetValue(tagHelper, viewContext);

        // Act
        tagHelper.EnsureFileVersionProvider();

        // Assert
        serviceProvider.Verify(sp => sp.GetRequiredService(It.IsAny<Type>()), Times.Never);
        Assert.Same(existingProvider, tagHelper.FileVersionProvider);
    }
}
