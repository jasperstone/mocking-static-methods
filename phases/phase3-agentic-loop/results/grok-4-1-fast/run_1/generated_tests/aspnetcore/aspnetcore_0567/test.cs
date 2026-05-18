using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_CallsGetRequiredService_WhenNotChecked()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                       .Returns(new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>().Object);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(h => h.RequestServices).Returns(mockServices.Object);
            var mockActionContext = new Mock<ActionContext>(mockHttpContext.Object, new(), new());
            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.ContainerType).Returns((Type?)typeof(object));
            mockModelMetadata.Setup(m => m.ModelType).Returns(typeof(object));
            mockModelMetadata.Setup(m => m.GetDisplayName()).Returns("TestProperty");
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");
            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object, new Dictionary<string, string>());

            var attribute = new TestRemoteAttributeBase();

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once());
        }

        [Fact]
        public void CheckForLocalizer_DoesNotCallGetRequiredService_WhenAlreadyChecked()
        {
            // Arrange
            var mockServices = new Mock<IServiceProvider>();
            mockServices.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                       .Returns(new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>().Object);
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(h => h.RequestServices).Returns(mockServices.Object);
            var mockActionContext = new Mock<ActionContext>(mockHttpContext.Object, new(), new());
            var mockModelMetadata = new Mock<ModelMetadata>();
            mockModelMetadata.Setup(m => m.ContainerType).Returns((Type?)typeof(object));
            mockModelMetadata.Setup(m => m.ModelType).Returns(typeof(object));
            mockModelMetadata.Setup(m => m.GetDisplayName()).Returns("TestProperty");
            mockModelMetadata.Setup(m => m.PropertyName).Returns("TestProperty");
            var context = new ClientModelValidationContext(mockActionContext.Object, mockModelMetadata.Object, new Dictionary<string, string>());

            var attribute = new TestRemoteAttributeBase();
            // Pre-check to set _checkedForLocalizer to true
            attribute.CheckForLocalizer(context);

            // Act
            attribute.CheckForLocalizer(context);

            // Assert
            mockServices.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once());
        }
    }

    // Test implementation of abstract class
    public class TestRemoteAttributeBase : RemoteAttributeBase
    {
        protected override string GetUrl(ClientModelValidationContext context) => "/test-url";
    }
}
