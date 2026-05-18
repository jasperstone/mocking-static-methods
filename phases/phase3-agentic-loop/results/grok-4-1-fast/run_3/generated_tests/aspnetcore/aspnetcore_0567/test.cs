using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Test
{
    public class RemoteAttributeBaseTests
    {
        [Fact]
        public void CheckForLocalizer_CallsGetRequiredService()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new MvcDataAnnotationsLocalizationOptions());

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                    .Returns(mockOptions.Object);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(h => h.RequestServices).Returns(services.Object);

            var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
            var modelMetadata = Mock.Of<ModelMetadata>(m => m.PropertyName == "Property" && m.GetDisplayName() == "Property");

            var context = new ClientModelValidationContext(actionContext)
            {
                ModelMetadata = modelMetadata,
                Attributes = new Dictionary<string, string>()
            };

            var attribute = new TestRemoteAttribute();

            // Act
            attribute.AddValidation(context);

            // Assert
            services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }

        [Fact]
        public void CheckForLocalizer_CachesCheck()
        {
            // Arrange
            var mockOptions = new Mock<IOptions<MvcDataAnnotationsLocalizationOptions>>();
            mockOptions.Setup(o => o.Value).Returns(new MvcDataAnnotationsLocalizationOptions());

            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                    .Returns(mockOptions.Object);

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(h => h.RequestServices).Returns(services.Object);

            var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
            var modelMetadata = Mock.Of<ModelMetadata>(m => m.PropertyName == "Property" && m.GetDisplayName() == "Property");

            var context1 = new ClientModelValidationContext(actionContext)
            {
                ModelMetadata = modelMetadata,
                Attributes = new Dictionary<string, string>()
            };

            var context2 = new ClientModelValidationContext(actionContext)
            {
                ModelMetadata = modelMetadata,
                Attributes = new Dictionary<string, string>()
            };

            var attribute = new TestRemoteAttribute();

            // Act
            attribute.AddValidation(context1);
            attribute.AddValidation(context2);

            // Assert
            services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Once);
        }

        private class TestRemoteAttribute : RemoteAttributeBase
        {
            protected override string GetUrl(ClientModelValidationContext context) => "/test-url";
        }
    }
}
