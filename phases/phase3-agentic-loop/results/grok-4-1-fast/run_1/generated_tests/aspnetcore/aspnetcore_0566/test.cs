using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_CallsGetRequiredService_OnRequestServices()
        {
            // Arrange
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(h => h.RouteUrl(It.IsAny<UrlRouteContext>()))
                        .Returns("/test-url");
            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                               .Returns(mockUrlHelper.Object);

            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IUrlHelperFactory>())
                       .Returns(mockUrlHelperFactory.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServices.Object);

            var mockActionContext = new Mock<ActionContext>();
            mockActionContext.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");

            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object);

            var attribute = new RemoteAttribute("action", "controller");

            // Act
            var url = attribute.GetUrl(context);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IUrlHelperFactory>(), Times.Once());
            Assert.Equal("/test-url", url);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var mockUrlHelperFactory = new Mock<IUrlHelperFactory>();
            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(h => h.RouteUrl(It.IsAny<UrlRouteContext>()))
                        .Returns((string)null);

            mockUrlHelperFactory.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                               .Returns(mockUrlHelper.Object);

            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IUrlHelperFactory>())
                       .Returns(mockUrlHelperFactory.Object);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServices.Object);

            var mockActionContext = new Mock<ActionContext>();
            mockActionContext.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");

            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object);

            var attribute = new RemoteAttribute("action", "controller");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(context));
            Assert.Contains("No URL could be found", exception.Message);
        }

        [Fact]
        public void GetUrl_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var attribute = new RemoteAttribute("action", "controller");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.GetUrl(null!));
        }
    }
}
