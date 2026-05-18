using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_CallsGetRequiredServiceOnRequestServices()
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
                       .Returns(mockUrlHelperFactory.Object)
                       .Verifiable();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(c => c.RequestServices).Returns(mockServices.Object);

            var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var emptyModelMetadata = modelMetadataProvider.GetMetadataForProperty(typeof(object), "TestProperty");
            
            var clientValidationContext = new ClientModelValidationContext(actionContext, emptyModelMetadata, new ModelStateDictionary());

            var attribute = new RemoteAttribute("action", "controller");

            // Act
            var url = attribute.GetUrl(clientValidationContext);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IUrlHelperFactory>(), Times.Once);
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
            mockHttpContext.SetupGet(c => c.RequestServices).Returns(mockServices.Object);

            var actionContext = new ActionContext(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var emptyModelMetadata = modelMetadataProvider.GetMetadataForProperty(typeof(object), "TestProperty");
            
            var clientValidationContext = new ClientModelValidationContext(actionContext, emptyModelMetadata, new ModelStateDictionary());

            var attribute = new RemoteAttribute("action", "controller");

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(clientValidationContext));
            Assert.Contains("NoUrlFound", exception.Message);
        }

        [Fact]
        public void GetUrl_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Arrange
            var attribute = new TestRemoteAttribute();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => attribute.GetUrl(null!));
        }

        private class TestRemoteAttribute : RemoteAttribute
        {
            protected override string GetUrl(ClientModelValidationContext context) => throw new NotImplementedException();
        }
    }
}
