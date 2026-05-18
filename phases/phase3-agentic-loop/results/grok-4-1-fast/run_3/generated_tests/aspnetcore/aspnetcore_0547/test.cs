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

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests;

public class ScriptTagHelperTests
{
    private readonly Mock<IWebHostEnvironment> _mockHostingEnvironment;
    private readonly Mock<TagHelperMemoryCacheProvider> _mockCacheProvider;
    private readonly Mock<IFileVersionProvider> _mockFileVersionProvider;
    private readonly Mock<HtmlEncoder> _mockHtmlEncoder;
    private readonly Mock<JavaScriptEncoder> _mockJavaScriptEncoder;
    private readonly Mock<IUrlHelperFactory> _mockUrlHelperFactory;
    private ScriptTagHelper _tagHelper;

    public ScriptTagHelperTests()
    {
        _mockHostingEnvironment = new();
        _mockCacheProvider = new();
        _mockFileVersionProvider = new();
        _mockHtmlEncoder = new();
        _mockJavaScriptEncoder = new();
        _mockUrlHelperFactory = new();
        
        _mockCacheProvider.Setup(c => c.Cache).Returns(new MemoryCache(new MemoryCacheOptions()));
        
        _tagHelper = new ScriptTagHelper(
            _mockHostingEnvironment.Object,
            _mockCacheProvider.Object,
            null!,
            _mockHtmlEncoder.Object,
            _mockJavaScriptEncoder.Object,
            _mockUrlHelperFactory.Object);
    }

    [Fact]
    public void EnsureFileVersionProvider_CallsGetRequiredService_WhenFileVersionProviderIsNull()
    {
        // Arrange
        var mockRequestServices = new Mock<IServiceProvider>();
        mockRequestServices.Setup(s => s.GetRequiredService(typeof(IFileVersionProvider)))
                          .Returns(_mockFileVersionProvider.Object)
                          .Verifiable();

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockRequestServices.Object);

        var mockViewContext = new Mock<ViewContext>();
        mockViewContext.Setup(v => v.HttpContext).Returns(mockHttpContext.Object);

        _tagHelper.ViewContext = mockViewContext.Object;

        // Act
        _tagHelper.EnsureFileVersionProvider();

        // Assert
        mockRequestServices.Verify(s => s.GetRequiredService(typeof(IFileVersionProvider)), Times.Once);
        Assert.NotNull(_tagHelper.FileVersionProvider);
        Assert.Same(_mockFileVersionProvider.Object, _tagHelper.FileVersionProvider);
    }

    [Fact]
    public void EnsureFileVersionProvider_DoesNotCallGetRequiredService_WhenFileVersionProviderIsNotNull()
    {
        // Arrange
        var mockRequestServices = new Mock<IServiceProvider>();
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.RequestServices).Returns(mockRequestServices.Object);

        var mockViewContext = new Mock<ViewContext>();
        mockViewContext.Setup(v => v.HttpContext).Returns(mockHttpContext.Object);

        _tagHelper.FileVersionProvider = _mockFileVersionProvider.Object;
        _tagHelper.ViewContext = mockViewContext.Object;

        // Act
        _tagHelper.EnsureFileVersionProvider();

        // Assert
        mockRequestServices.Verify(s => s.GetRequiredService(typeof(IFileVersionProvider)), Times.Never);
    }
}
