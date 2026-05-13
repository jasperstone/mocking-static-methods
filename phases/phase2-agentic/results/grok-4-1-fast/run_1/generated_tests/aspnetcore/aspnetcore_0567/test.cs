using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc;

public class RemoteAttributeBaseTests
{
    [Fact]
    public void CheckForLocalizer_CallsGetRequiredService_WhenNotChecked()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        var options = new MvcDataAnnotationsLocalizationOptions();
        services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns(new OptionsManager<MvcDataAnnotationsLocalizationOptions>(new[] { new OptionsWrapper<MvcDataAnnotationsLocalizationOptions>(options) }));
        services.Setup(s => s.GetService<IStringLocalizerFactory>()).Returns((IStringLocalizerFactory)null);

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(h => h.RequestServices).Returns(services.Object);

        var actionContext = new ActionContext(httpContext.Object, new(), new());
        var modelMetadata = Mock.Of<IModelMetadata>(m => m.ModelType == typeof(string) && m.ContainerType == null && m.PropertyName == "TestProperty" && m.GetDisplayName() == "Test");
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
    public void CheckForLocalizer_DoesNotCallGetRequiredService_WhenAlreadyChecked()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Returns((IOptions<MvcDataAnnotationsLocalizationOptions>)null);

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(h => h.RequestServices).Returns(services.Object);

        var actionContext = new ActionContext(httpContext.Object, new(), new());
        var modelMetadata = Mock.Of<IModelMetadata>();
        var context = new ClientModelValidationContext(actionContext)
        {
            ModelMetadata = modelMetadata,
            Attributes = new Dictionary<string, string>()
        };

        var attribute = new TestRemoteAttribute();
        // Pre-check to set _checkedForLocalizer = true
        attribute.CheckForLocalizer(context);

        // Act
        attribute.AddValidation(context);

        // Assert
        services.Verify(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>(), Times.Never);
    }

    [Fact]
    public void CheckForLocalizer_HandlesGetRequiredServiceException_Gracefully()
    {
        // Arrange
        var services = new Mock<IServiceProvider>();
        services.Setup(s => s.GetRequiredService<IOptions<MvcDataAnnotationsLocalizationOptions>>())
                .Throws(new InvalidOperationException("Service not registered"));

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(h => h.RequestServices).Returns(services.Object);

        var actionContext = new ActionContext(httpContext.Object, new(), new());
        var modelMetadata = Mock.Of<IModelMetadata>();
        var context = new ClientModelValidationContext(actionContext)
        {
            ModelMetadata = modelMetadata,
            Attributes = new Dictionary<string, string>()
        };

        var attribute = new TestRemoteAttribute();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => attribute.AddValidation(context));
        Assert.Equal("Service not registered", exception.Message);
    }

    private class TestRemoteAttribute : RemoteAttributeBase
    {
        protected override string GetUrl(ClientModelValidationContext context) => "/test-url";
    }
}
