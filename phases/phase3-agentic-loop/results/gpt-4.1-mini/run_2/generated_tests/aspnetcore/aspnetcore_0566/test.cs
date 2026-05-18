using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc
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

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProviderMock.Object;

            var actionContext = new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                new ModelMetadata(),
                new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("actionName", "controllerName");

            // Act
            var url = remoteAttribute.GetUrl(clientModelValidationContext);

            // Assert
            Assert.Equal(expectedUrl, url);

            urlHelperFactoryMock.Verify(f => f.GetUrlHelper(actionContext), Times.Once);
            urlHelperMock.Verify(u => u.RouteUrl(It.Is<UrlRouteContext>(ctx =>
                ctx.RouteName == null &&
                ctx.Values.ContainsKey("action") && (string)ctx.Values["action"] == "actionName" &&
                ctx.Values.ContainsKey("controller") && (string)ctx.Values["controller"] == "controllerName"
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

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = serviceProviderMock.Object;

            var actionContext = new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            var clientModelValidationContext = new ClientModelValidationContext(
                actionContext,
                new ModelMetadata(),
                new Dictionary<string, string>());

            var remoteAttribute = new RemoteAttribute("actionName", "controllerName");

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => remoteAttribute.GetUrl(clientModelValidationContext));
            Assert.Equal(Resources.RemoteAttribute_NoUrlFound, ex.Message);
        }
    }

    // Minimal stub for ClientModelValidationContext to allow testing
    internal class ClientModelValidationContext
    {
        public ActionContext ActionContext { get; }
        public ModelMetadata ModelMetadata { get; }
        public IDictionary<string, string> Attributes { get; }

        public ClientModelValidationContext(ActionContext actionContext, ModelMetadata modelMetadata, IDictionary<string, string> attributes)
        {
            ActionContext = actionContext;
            ModelMetadata = modelMetadata;
            Attributes = attributes;
        }
    }

    // Minimal stub for ModelMetadata to allow testing
    internal class ModelMetadata
    {
        public string? PropertyName { get; set; }
        public string GetDisplayName() => PropertyName ?? string.Empty;
    }
}
