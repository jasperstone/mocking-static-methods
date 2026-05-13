using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_UsesGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();
            var actionContextMock = new Mock<ActionContext>();
            var httpContextMock = new Mock<HttpContext>();
            var requestServicesMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(p => p.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactoryMock.Object);
            urlHelperFactoryMock.Setup(p => p.GetUrlHelper(actionContextMock.Object)).Returns(urlHelperMock.Object);
            actionContextMock.Setup(p => p.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.Setup(p => p.RequestServices).Returns(requestServicesMock.Object);

            var remoteAttribute = new RemoteAttribute("action", "controller");
            var context = new ClientModelValidationContext
            {
                ActionContext = actionContextMock.Object,
            };

            // Act
            remoteAttribute.GetUrl(context);

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IUrlHelperFactory>(), Times.Once);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            var urlHelperMock = new Mock<IUrlHelper>();
            var actionContextMock = new Mock<ActionContext>();
            var httpContextMock = new Mock<HttpContext>();
            var requestServicesMock = new Mock<IServiceProvider>();

            serviceProviderMock.Setup(p => p.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactoryMock.Object);
            urlHelperFactoryMock.Setup(p => p.GetUrlHelper(actionContextMock.Object)).Returns(urlHelperMock.Object);
            actionContextMock.Setup(p => p.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.Setup(p => p.RequestServices).Returns(requestServicesMock.Object);
            urlHelperMock.Setup(p => p.RouteUrl(It.IsAny<UrlRouteContext>())).Returns((string?)null);

            var remoteAttribute = new RemoteAttribute("action", "controller");
            var context = new ClientModelValidationContext
            {
                ActionContext = actionContextMock.Object,
            };

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(context));
        }
    }
}
