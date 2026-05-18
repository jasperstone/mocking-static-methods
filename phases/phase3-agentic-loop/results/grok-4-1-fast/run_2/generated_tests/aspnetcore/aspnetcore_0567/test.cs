using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Test
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_CallsGetRequiredService_WhenNotChecked()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            var options = new MvcDataAnnotationsLocalizationOptions();
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                       .Returns(Options.Create(options));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServices.Object);

            var mockActionContext = new Mock<ActionContext>(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");
            mockModelMetadata.Setup(m => m.ContainerType).Returns((Type?)null);
            mockModelMetadata.Setup(m => m.ModelType).Returns(typeof(string));
            mockModelMetadata.Setup(m => m.GetDisplayName()).Returns("Test Display Name");
            
            var provider = new EmptyModelMetadataProvider();
            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object, new Dictionary<string, string>(), provider);

            var attribute = new TestRemoteAttribute();

            // Act
            attribute.AddValidation(context);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once());
        }

        [Fact]
        public void CheckForLocalizer_DoesNotCallGetRequiredService_WhenAlreadyChecked()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            var options = new MvcDataAnnotationsLocalizationOptions();
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                       .Returns(Options.Create(options));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServices.Object);

            var mockActionContext = new Mock<ActionContext>(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");
            mockModelMetadata.Setup(m => m.ContainerType).Returns((Type?)null);
            mockModelMetadata.Setup(m => m.ModelType).Returns(typeof(string));
            mockModelMetadata.Setup(m => m.GetDisplayName()).Returns("Test Display Name");
            
            var provider = new EmptyModelMetadataProvider();
            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object, new Dictionary<string, string>(), provider);

            var attribute = new TestRemoteAttribute();
            attribute.CheckForLocalizer(context); // First call sets _checkedForLocalizer = true

            // Act
            attribute.AddValidation(context);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once());
        }

        [Fact]
        public void CheckForLocalizer_ThrowsInvalidOperationException_WhenServiceMissing()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                       .Throws(new InvalidOperationException("No service for type 'IOptions`1[MvcDataAnnotationsLocalizationOptions]' has been registered."));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.RequestServices).Returns(mockServices.Object);

            var mockActionContext = new Mock<ActionContext>(mockHttpContext.Object, new RouteData(), new ActionDescriptor());
            
            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");
            
            var provider = new EmptyModelMetadataProvider();
            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object, new Dictionary<string, string>(), provider);

            var attribute = new TestRemoteAttribute();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => attribute.CheckForLocalizer(context));
            Assert.Equal("No service for type 'IOptions`1[MvcDataAnnotationsLocalizationOptions]' has been registered.", ex.Message);
        }

        private class TestRemoteAttribute : RemoteAttributeBase
        {
            protected override string GetUrl(ClientModelValidationContext context) => "/test-url";
        }
    }
}
