using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class RemoteAttributeTests
{
    [Fact]
    public void GetUrl_ShouldGenerateUrl_WhenServiceProvided()
    {
        // Arrange
        var routeData = new RouteData();
        var routeName = "TestRoute";
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = routeData
        };
        var context = new ClientModelValidationContext(actionContext, new ModelMetadataProvider(), new EmptyModelMetadataProvider(), null, null);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(helper => helper.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns("http://example.com");

        var factoryMock = new Mock<IUrlHelperFactory>();
        factoryMock
            .Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock
            .Setup(s => s.GetRequiredService<IUrlHelperFactory>())
            .Returns(factoryMock.Object);

        var attribute = new RemoteAttribute(null, null)
        {
            RouteName = routeName
        };

        // Act
        var url = attribute.GetUrl(context);

        // Assert
        Assert.Equal("http://example.com", url);
        factoryMock.Verify(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
        urlHelperMock.Verify(helper => helper.RouteUrl(It.Is<UrlRouteContext>(ctx => ctx.RouteName == routeName && ctx.Values == routeData)), Times.Once);
    }

    [Fact]
    public void GetUrl_ShouldThrowInvalidOperationException_WhenUrlIsNotGenerated()
    {
        // Arrange
        var routeData = new RouteData();
        var routeName = "TestRoute";
        var actionContext = new ActionContext
        {
            HttpContext = new DefaultHttpContext(),
            RouteData = routeData
        };
        var context = new ClientModelValidationContext(actionContext, new ModelMetadataProvider(), new EmptyModelMetadataProvider(), null, null);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(helper => helper.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns((string)null);

        var factoryMock = new Mock<IUrlHelperFactory>();
        factoryMock
            .Setup(factory => factory.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var servicesMock = new Mock<IServiceProvider>();
        servicesMock
            .Setup(s => s.GetRequiredService<IUrlHelperFactory>())
            .Returns(factoryMock.Object);

        var attribute = new RemoteAttribute(null, null)
        {
            RouteName = routeName
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(context));
        Assert.Equal("No URL found", exception.Message);
    }
}
