using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RemoteAttributeTests
{
    public class RemoteAttributeTest
    {
        [Fact]
        public void GetUrl_Should_Call_GetRequiredService_And_Return_Url()
        {
            // Arrange
            var routeData = new RouteValueDictionary
            {
                { "action", "TestAction" },
                { "controller", "TestController" },
                { "area", "TestArea" }
            };

            var requestServicesMock = new Mock<IServiceProvider>();
            var urlHelperMock = new Mock<IUrlHelper>();
            var factoryMock = new Mock<IUrlHelperFactory>();
            var httpContextMock = new DefaultHttpContext();

            // Setup RequestServices to return a service provider that returns urlHelperMock for GetRequiredService
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IUrlHelperFactory>(factoryMock.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            httpContextMock.RequestServices = serviceProvider;

            // Setup factory to return urlHelperMock
            factoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);

            // Setup urlHelper to return a URL string
            var expectedUrl = "http://testurl.com";
            urlHelperMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns(expectedUrl);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(routeData),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var context = new ClientModelValidationContext
            {
                ActionContext = actionContext
            };

            var remoteAttribute = new RemoteAttribute
            {
                RouteName = "TestRoute"
            };
            remoteAttribute.RouteData["action"] = "TestAction";
            remoteAttribute.RouteData["controller"] = "TestController";

            // Act
            var url = remoteAttribute.GetUrl(context);

            // Assert
            Assert.Equal(expectedUrl, url);
            factoryMock.Verify(f => f.GetUrlHelper(It.Is<ActionContext>(ac => ac == actionContext)), Times.Once);
            urlHelperMock.Verify(u => u.RouteUrl(It.Is<UrlRouteContext>(ur => ur.RouteName == "TestRoute")), Times.Once);
        }
    }
}
