using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_ReturnsCorrectUrl()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("http://example.com");

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var context = new ClientModelValidationContext(actionContext, new ModelMetadata(new EmptyModelMetadataProvider()), new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("action", "controller", "area");

            // Act
            var url = remoteAttribute.GetUrl(context);

            // Assert
            Assert.Equal("http://example.com", url);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns((string)null);

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();
            urlHelperFactoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IUrlHelperFactory>()).Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(hc => hc.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object,
                RouteData = new RouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            var context = new ClientModelValidationContext(actionContext, new ModelMetadata(new EmptyModelMetadataProvider()), new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("action", "controller", "area");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(context));
        }
    }
}
