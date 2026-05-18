using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_ShouldReturnUrl_WhenUrlIsGenerated()
        {
            // Arrange
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();
            var mockClientModelValidationContext = new Mock<ClientModelValidationContext>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory))).Returns(mockUrlHelperFactory.Object);
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);
            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);
            mockClientModelValidationContext.Setup(cmvc => cmvc.ActionContext).Returns(mockActionContext.Object);

            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);
            mockUrlHelper.Setup(uh => uh.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("http://example.com");

            var remoteAttribute = new RemoteAttribute("action", "controller", "area");

            // Act
            var url = remoteAttribute.GetUrl(mockClientModelValidationContext.Object);

            // Assert
            Assert.Equal("http://example.com", url);
        }

        [Fact]
        public void GetUrl_ShouldThrowInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockHttpContext = new Mock<HttpContext>();
            var mockActionContext = new Mock<ActionContext>();
            var mockClientModelValidationContext = new Mock<ClientModelValidationContext>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IUrlHelperFactory))).Returns(mockUrlHelperFactory.Object);
            mockHttpContext.Setup(hc => hc.RequestServices).Returns(mockServiceProvider.Object);
            mockActionContext.Setup(ac => ac.HttpContext).Returns(mockHttpContext.Object);
            mockClientModelValidationContext.Setup(cmvc => cmvc.ActionContext).Returns(mockActionContext.Object);

            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);
            mockUrlHelper.Setup(uh => uh.RouteUrl(It.IsAny<UrlRouteContext>())).Returns((string)null);

            var remoteAttribute = new RemoteAttribute("action", "controller", "area");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(mockClientModelValidationContext.Object));
            Assert.Equal("No URL found for the remote validation.", exception.Message);
        }
    }
}
