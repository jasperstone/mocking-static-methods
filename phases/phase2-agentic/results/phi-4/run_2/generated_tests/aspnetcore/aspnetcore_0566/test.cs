using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class RemoteAttributeTests
{
    [Fact]
    public void GetUrl_GeneratesCorrectUrl_WhenServiceProvided()
    {
        // Arrange
        var routeName = "testRoute";
        var routeData = new Microsoft.AspNetCore.Routing.RouteValueDictionary
        {
            { "controller", "TestController" },
            { "action", "TestAction" }
        };

        var context = new ClientModelValidationContext(
            new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor()),
            new[] { new ModelMetadataIdentity("TestModel", "TestProperty") },
            null);

        var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
        var urlHelperMock = new Mock<UrlHelper>();
        urlHelperMock
            .Setup(uh => uh.RouteUrl(It.IsAny<UrlRouteContext>()))
            .Returns("http://localhost/test");

        urlHelperFactoryMock
            .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
            .Returns(urlHelperMock.Object);

        var remoteAttribute = new RemoteAttribute(null, null)
        {
            RouteName = routeName
        };

        // Act
        var url = remoteAttribute.GetUrl(context);

        // Assert
        Assert.Equal("http://localhost/test", url);
        urlHelperFactoryMock.Verify(f => f.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
        urlHelperMock.Verify(uh => uh.RouteUrl(It.Is<UrlRouteContext>(ctx => 
            ctx.RouteName == routeName && ctx.Values == routeData)), Times.Once);
    }
}
