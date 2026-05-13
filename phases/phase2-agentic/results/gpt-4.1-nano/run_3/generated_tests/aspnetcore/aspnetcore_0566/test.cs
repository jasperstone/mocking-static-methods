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

            var routeData = new RouteData();
            var remoteAttr = new RemoteAttribute("action", "controller", "area");
            remoteAttr.RouteData = routeData;

            // Setup the factory to return the mock URL helper
            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(mockUrlHelper.Object);

            // Setup the URL helper to return a specific URL
            var expectedUrl = "/test/url";
            mockUrlHelper.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(expectedUrl);

            // Act
            var result = remoteAttr.GetUrl(mockClientValidationContext);

            // Assert
            Assert.Equal(expectedUrl, result);
            mockUrlHelperFactory.Verify(f => f.GetUrlHelper(It.IsAny<ActionContext>()), Times.Once);
            mockUrlHelper.Verify(u => u.RouteUrl(It.IsAny<UrlRouteContext>()), Times.Once);
        }
    }
}
