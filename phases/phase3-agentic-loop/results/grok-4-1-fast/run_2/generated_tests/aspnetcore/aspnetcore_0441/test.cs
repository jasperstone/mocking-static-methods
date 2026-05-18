using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test;

public class ControllerBaseTests
{
    [Fact]
    public void Url_Get_ReturnsCachedValue_WhenAlreadySet()
    {
        // Arrange
        var controller = new TestController();
        var expectedUrlHelper = Mock.Of<IUrlHelper>();
        controller.Url = expectedUrlHelper;

        // Act
        var result = controller.Url;

        // Assert
        Assert.Same(expectedUrlHelper, result);
    }

    [Fact]
    public void Url_Get_CreatesUrlHelper_WhenNotCached_AndServicesAvailable()
    {
        // Arrange
        var factoryMock = new Mock<IUrlHelperFactory>();
        var urlHelper = Mock.Of<IUrlHelper>();
        factoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ControllerContext>())).Returns(urlHelper);

        var services = new ServiceCollection();
        services.AddSingleton<IUrlHelperFactory>(factoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var controllerContext = new ControllerContext { HttpContext = httpContext };
        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act
        var result = controller.Url;

        // Assert
        Assert.Same(urlHelper, result);
        factoryMock.Verify(f => f.GetUrlHelper(controller.ControllerContext), Times.Once);
    }

    [Fact]
    public void Url_Get_ReturnsNull_WhenHttpContextNull()
    {
        // Arrange
        var controller = new TestController();

        // Act & Assert
        Assert.Null(controller.Url);
    }

    [Fact]
    public void Url_Get_ReturnsNull_WhenRequestServicesNull()
    {
        // Arrange
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns((IServiceProvider?)null);
        var httpContext = httpContextMock.Object;
        var controllerContext = new ControllerContext { HttpContext = httpContext };
        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act & Assert
        Assert.Null(controller.Url);
    }

    [Fact]
    public void Url_Get_ThrowsInvalidOperationException_WhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = serviceProvider;
        var controllerContext = new ControllerContext { HttpContext = httpContext };
        var controller = new TestController();
        controller.ControllerContext = controllerContext;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => controller.Url);
        Assert.Contains("IUrlHelperFactory", exception.Message);
    }

    [Fact]
    public void Url_Set_ThrowsArgumentNullException_WhenValueIsNull()
    {
        // Arrange
        var controller = new TestController();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => controller.Url = null!);
    }
}

public class TestController : ControllerBase
{
}
