using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_UsesIUrlHelperFactoryToGenerateUrl_ReturnsUrl()
        {
            // Arrange
            var expectedUrl = "http://test-url";
            var routeName = "routeName";

            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(expectedUrl);

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            // IServiceProvider extension method GetRequiredService<T> calls GetService internally,
            // so we mock GetService here. We will use the extension method in the test.

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object
            };

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                new Mock<ModelMetadata>(MockBehavior.Strict).Object,
                new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute(routeName);

            // Act
            var url = remoteAttribute.GetUrl(clientModelValidationContext);

            // Assert
            Assert.Equal(expectedUrl, url);

            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(actionContext), Times.Once);
            urlHelperMock.Verify(u => u.RouteUrl(It.Is<UrlRouteContext>(ctx => ctx.RouteName == routeName)), Times.Once);
        }

        [Fact]
        public void GetUrl_UrlHelperReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns((string?)null);

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object);

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object
            };

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                new Mock<ModelMetadata>(MockBehavior.Strict).Object,
                new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("routeName");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(clientModelValidationContext));
            Assert.Equal(Resources.RemoteAttribute_NoUrlFound, ex.Message);
        }
    }
}
