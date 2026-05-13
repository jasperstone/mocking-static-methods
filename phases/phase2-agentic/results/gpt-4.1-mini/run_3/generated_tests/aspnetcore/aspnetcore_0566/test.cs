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
        public void GetUrl_ReturnsUrl_FromUrlHelper()
        {
            // Arrange
            var expectedUrl = "http://test-url";
            var routeData = new RouteValueDictionary();
            var remoteAttribute = new RemoteAttribute("action", "controller");

            var urlHelperMock = new Mock<IUrlHelper>(MockBehavior.Strict);
            urlHelperMock
                .Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>()))
                .Returns(expectedUrl)
                .Verifiable();

            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>(MockBehavior.Strict);
            urlHelperFactoryMock
                .Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>()))
                .Returns(urlHelperMock.Object)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IUrlHelperFactory)))
                .Returns(urlHelperFactoryMock.Object);

            // IServiceProvider extension method GetRequiredService<T> calls GetService internally,
            // so we mock GetService here. We will use the extension method in the tested code.

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object,
                RouteData = new RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(string));

            var clientModelValidationContext = new ClientModelValidationContext(
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadataIdentity(),
                modelMetadata,
                actionContext,
                new System.Collections.Generic.Dictionary<string, string>());

            // Act
            var url = remoteAttribute.GetUrl(clientModelValidationContext);

            // Assert
            Assert.Equal(expectedUrl, url);
            urlHelperFactoryMock.Verify();
            urlHelperMock.Verify();
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
        {
            // Arrange
            var remoteAttribute = new RemoteAttribute("action", "controller");

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
                RouteData = new RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(string));

            var clientModelValidationContext = new ClientModelValidationContext(
                new Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadataIdentity(),
                modelMetadata,
                actionContext,
                new System.Collections.Generic.Dictionary<string, string>());

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(clientModelValidationContext));
            Assert.Equal(Resources.RemoteAttribute_NoUrlFound, ex.Message);
        }
    }
}
