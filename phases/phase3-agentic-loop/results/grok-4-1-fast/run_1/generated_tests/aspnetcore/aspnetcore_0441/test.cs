using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests;

public class ControllerBaseTests
{
    [Fact]
    public void Url_Get_CachesValue_WhenServiceAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockFactory = new Mock<IUrlHelperFactory>();
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockFactory.Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>())).Returns(mockUrlHelper.Object);
        services.AddSingleton(mockFactory.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act
        var url1 = controller.Url;
        var url2 = controller.Url;

        // Assert
        Assert.NotNull(url1);
        Assert.Same(url1, url2);
        mockFactory.Verify(f => f.GetUrlHelper(controllerContext), Times.Once);
    }

    [Fact]
    public void Url_Get_ReturnsNull_WhenHttpContextNull()
    {
        // Arrange
        var controllerContext = new ControllerContext()
        {
            HttpContext = null!
        };

        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act & Assert
        Assert.Null(controller.Url);
    }

    [Fact]
    public void Url_Get_ReturnsNull_WhenRequestServicesNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext()
        {
            RequestServices = null!
        };

        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act & Assert
        Assert.Null(controller.Url);
    }

    [Fact]
    public void Url_Get_ThrowsInvalidOperationException_WhenServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };

        var controllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => controller.Url);
        Assert.Contains("Unable to resolve service for type 'IUrlHelperFactory'", exception.Message);
    }

    [Fact]
    public void Url_Set_OverridesCachedValue()
    {
        // Arrange
        var mockUrl1 = new Mock<IUrlHelper>().Object;
        var mockUrl2 = new Mock<IUrlHelper>().Object;

        var controller = new TestController();
        controller.Url = mockUrl1;

        // Act
        controller.Url = mockUrl2;

        // Assert
        Assert.Same(mockUrl2, controller.Url);
    }

    private class TestController : ControllerBase
    {
    }
}
