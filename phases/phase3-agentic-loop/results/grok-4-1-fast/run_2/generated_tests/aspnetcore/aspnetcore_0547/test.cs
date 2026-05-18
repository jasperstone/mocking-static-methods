using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

public class ScriptTagHelperEnsureFileVersionProviderTests
{
    [Fact]
    public void EnsureFileVersionProvider_CallsGetRequiredService_WhenNull()
    {
        // Arrange
        var mockFileVersionProvider = new Mock<IFileVersionProvider>();
        var requestServices = new Mock<IServiceProvider>();
        requestServices.Setup(s => s.GetRequiredService(typeof(IFileVersionProvider)))
                      .Returns(mockFileVersionProvider.Object);

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = requestServices.Object
        };
        var actionContext = new ActionContext(httpContext, new(), new());
        var viewContext = new ViewContext(
            actionContext, 
            Mock.Of<IRazorPage>(), 
            new EmptyModelMetadataProvider(), 
            new ModelStateDictionary(), 
            TextWriter.Null,
            new HtmlHelperOptions());

        var hostingEnvironment = Mock.Of<IWebHostEnvironment>();
        var cacheProvider = new TagHelperMemoryCacheProvider(Mock.Of<IMemoryCache>());
        var scriptTagHelper = new ScriptTagHelper(
            hostingEnvironment,
            cacheProvider,
            fileVersionProvider: null,
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.HtmlEncoder>(),
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.JavaScriptEncoder>(),
            Mock.Of<IUrlHelperFactory>());

        scriptTagHelper.ViewContext = viewContext;

        // Act
        scriptTagHelper.EnsureFileVersionProvider();

        // Assert
        Assert.NotNull(scriptTagHelper.FileVersionProvider);
        requestServices.Verify(s => s.GetRequiredService(typeof(IFileVersionProvider)), Times.Once);
    }

    [Fact]
    public void EnsureFileVersionProvider_DoesNotCallGetRequiredService_WhenAlreadySet()
    {
        // Arrange
        var requestServices = new Mock<IServiceProvider>();
        var existingFileVersionProvider = Mock.Of<IFileVersionProvider>();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = requestServices.Object
        };
        var actionContext = new ActionContext(httpContext, new(), new());
        var viewContext = new ViewContext(
            actionContext,
            Mock.Of<IRazorPage>(),
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary(),
            TextWriter.Null,
            new HtmlHelperOptions());

        var hostingEnvironment = Mock.Of<IWebHostEnvironment>();
        var cacheProvider = new TagHelperMemoryCacheProvider(Mock.Of<IMemoryCache>());
        var scriptTagHelper = new ScriptTagHelper(
            hostingEnvironment,
            cacheProvider,
            existingFileVersionProvider,
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.HtmlEncoder>(),
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.JavaScriptEncoder>(),
            Mock.Of<IUrlHelperFactory>());

        scriptTagHelper.ViewContext = viewContext;

        // Act
        scriptTagHelper.EnsureFileVersionProvider();

        // Assert
        requestServices.Verify(s => s.GetRequiredService(It.IsAny<Type>()), Times.Never);
    }

    [Fact]
    public void EnsureFileVersionProvider_ThrowsInvalidOperation_WhenRequestServicesNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext() { RequestServices = null };
        var actionContext = new ActionContext(httpContext, new(), new());
        var viewContext = new ViewContext(
            actionContext,
            Mock.Of<IRazorPage>(),
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary(),
            TextWriter.Null,
            new HtmlHelperOptions());

        var scriptTagHelper = new ScriptTagHelper(
            Mock.Of<IWebHostEnvironment>(),
            new TagHelperMemoryCacheProvider(Mock.Of<IMemoryCache>()),
            null,
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.HtmlEncoder>(),
            Mock.Of<Microsoft.AspNetCore.Text.Encodings.Web.JavaScriptEncoder>(),
            Mock.Of<IUrlHelperFactory>());

        scriptTagHelper.ViewContext = viewContext;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => scriptTagHelper.EnsureFileVersionProvider());
        Assert.Contains("IServiceProvider", exception.Message);
    }
}
