using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.AspNetCore.Mvc
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_Should_Call_GetRequiredService_And_Return_Url()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockServices = new ServiceCollection()
                .AddTransient(_ => mockUrlHelperFactory.Object)
                .BuildServiceProvider();

            var mockHttpContext = new DefaultHttpContext
            {
                RequestServices = mockServices
            };

            var mockActionContext = new ActionContext
            {
                HttpContext = mockHttpContext
            };

            var mockClientValidationContext = new ClientModelValidationContext
            {
                ActionContext = mockActionContext
            };

            var routeData = new RouteValueDictionary
            {
                { "action", "TestAction" },
                { "controller", "TestController" }
            };

            var remoteAttr = new RemoteAttribute("routeName");
            remoteAttr.RouteData["action"] = "TestAction";
            remoteAttr.RouteData["controller"] = "TestController";

            mockUrlHelper.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns("http://testurl");

            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            // Act
            var url = remoteAttr.GetUrl(mockClientValidationContext);

            // Assert
            Assert.Equal("http://testurl", url);
            mockUrlHelperFactory.Verify(f => f.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
            mockUrlHelper.Verify(u => u.RouteUrl(It.IsAny<UrlRouteContext>()), Times.Once);
        }

        [Fact]
        public void GetUrl_Should_Throw_When_Url_Is_Null()
        {
            // Arrange
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockServices = new ServiceCollection()
                .AddTransient(_ => mockUrlHelperFactory.Object)
                .BuildServiceProvider();

            var mockHttpContext = new DefaultHttpContext
            {
                RequestServices = mockServices
            };

            var mockActionContext = new ActionContext
            {
                HttpContext = mockHttpContext
            };

            var mockClientValidationContext = new ClientModelValidationContext
            {
                ActionContext = mockActionContext
            };

            var remoteAttr = new RemoteAttribute("routeName");
            remoteAttr.RouteData["action"] = "TestAction";
            remoteAttr.RouteData["controller"] = "TestController";

            mockUrlHelper.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns<string>(null);

            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => remoteAttr.GetUrl(mockClientValidationContext));
        }
    }
}
