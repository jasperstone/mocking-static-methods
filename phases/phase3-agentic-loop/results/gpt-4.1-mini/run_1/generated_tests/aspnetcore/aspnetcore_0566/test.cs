using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Test
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_UsesServiceProviderToGetUrlHelperFactory_AndGeneratesUrl()
        {
            // Arrange
            var expectedUrl = "http://example.com/remote-validation";

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

            var httpContextMock = new Mock<Microsoft.AspNetCore.Http.HttpContext>(MockBehavior.Strict);
            httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProviderMock.Object);

            var actionContext = new ActionContext
            {
                HttpContext = httpContextMock.Object
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(object));

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                new System.Collections.Generic.Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("TestAction", "TestController");

            // Act
            var getUrlMethod = typeof(RemoteAttribute).GetMethod("GetUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var url = getUrlMethod!.Invoke(remoteAttribute, new object[] { clientModelValidationContext }) as string;

            // Assert
            Assert.Equal(expectedUrl, url);
            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(actionContext), Times.Once);
            urlHelperMock.Verify(u => u.RouteUrl(It.IsAny<UrlRouteContext>()), Times.Once);
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
                HttpContext = httpContextMock.Object
            };

            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = modelMetadataProvider.GetMetadataForType(typeof(object));

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                new System.Collections.Generic.Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("TestAction", "TestController");

            // Act & Assert
            var getUrlMethod = typeof(RemoteAttribute).GetMethod("GetUrl", BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Assert.Throws<TargetInvocationException>(() =>
                getUrlMethod!.Invoke(remoteAttribute, new object[] { clientModelValidationContext }));

            Assert.IsType<InvalidOperationException>(exception.InnerException);
            Assert.Equal(Resources.RemoteAttribute_NoUrlFound, exception.InnerException!.Message);
        }
    }
}
