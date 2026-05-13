using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_UsesServiceProviderToGetUrlHelperFactory_AndReturnsUrl()
        {
            // Arrange
            var expectedUrl = "http://example.com/route";

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

            // IServiceProvider extension method GetRequiredService<T> calls GetService and throws if null
            // We simulate this by setting up GetService to return the factory mock.

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object,
            };

            var clientModelValidationContext = new ClientModelValidationContext(
                new Mock<ModelMetadata>(MockBehavior.Strict).Object,
                actionContext,
                new System.Collections.Generic.Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("action", "controller");

            // Act
            var url = remoteAttribute.GetUrl(clientModelValidationContext);

            // Assert
            Assert.Equal(expectedUrl, url);

            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(actionContext), Times.Once);
            urlHelperMock.Verify(u => u.RouteUrl(It.Is<UrlRouteContext>(ctx =>
                ctx.RouteName == null &&
                ctx.Values != null &&
                ctx.Values["action"]?.ToString() == "action" &&
                ctx.Values["controller"]?.ToString() == "controller"
            )), Times.Once);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
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
                HttpContext = httpContextMock.Object,
            };

            var clientModelValidationContext = new ClientModelValidationContext(
                new Mock<ModelMetadata>(MockBehavior.Strict).Object,
                actionContext,
                new System.Collections.Generic.Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("action", "controller");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(clientModelValidationContext));
            Assert.Equal(Resources.RemoteAttribute_NoUrlFound, ex.Message);
        }
    }
}
