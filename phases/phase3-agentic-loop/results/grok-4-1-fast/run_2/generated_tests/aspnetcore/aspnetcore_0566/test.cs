using System.ComponentModel.DataAnnotations;
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

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Test;

public class RemoteAttributeTests
{
    [Fact]
    public void GetUrl_CallsGetRequiredServiceOnRequestServices()
    {
        // Arrange
        var servicesMock = new Mock<IServiceProvider>();
        var factoryMock = new Mock<IUrlHelperFactory>();
        var urlHelperMock = new Mock<IUrlHelper>();
        var actionContext = new ActionContext();
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(h => h.RequestServices).Returns(servicesMock.Object);
        actionContext.HttpContext = httpContextMock.Object;

        var modelMetadataMock = new Mock<ModelMetadata>();
        modelMetadataMock.Setup(m => m.PropertyName).Returns("TestProperty");
        var validationMetadataProviderMock = new Mock<IValidationMetadataProvider>();

        servicesMock.Setup(s => s.GetRequiredService<IUrlHelperFactory>()).Returns(factoryMock.Object);
        factoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        urlHelperMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns("/test-url");

        var context = new ClientModelValidationContext(actionContext)
        {
            ModelMetadata = modelMetadataMock.Object,
            ValidationMetadataProvider = validationMetadataProviderMock.Object
        };

        var attribute = new TestableRemoteAttribute();

        // Act
        var url = attribute.GetUrl(context);

        // Assert
        servicesMock.Verify(s => s.GetRequiredService<IUrlHelperFactory>(), Times.Once());
        Assert.Equal("/test-url", url);
    }

    [Fact]
    public void GetUrl_ThrowsArgumentNullException_WhenContextIsNull()
    {
        var attribute = new TestableRemoteAttribute();

        Assert.Throws<ArgumentNullException>(() => attribute.GetUrl(null!));
    }

    [Fact]
    public void GetUrl_ThrowsInvalidOperationException_WhenUrlIsNull()
    {
        // Arrange
        var servicesMock = new Mock<IServiceProvider>();
        var factoryMock = new Mock<IUrlHelperFactory>();
        var urlHelperMock = new Mock<IUrlHelper>();
        var actionContext = new ActionContext();
        var httpContextMock = new Mock<HttpContext>();

        httpContextMock.Setup(h => h.RequestServices).Returns(servicesMock.Object);
        actionContext.HttpContext = httpContextMock.Object;

        var modelMetadataMock = new Mock<ModelMetadata>();
        modelMetadataMock.Setup(m => m.PropertyName).Returns("TestProperty");
        var validationMetadataProviderMock = new Mock<IValidationMetadataProvider>();

        servicesMock.Setup(s => s.GetRequiredService<IUrlHelperFactory>()).Returns(factoryMock.Object);
        factoryMock.Setup(f => f.GetUrlHelper(It.IsAny<ActionContext>())).Returns(urlHelperMock.Object);
        urlHelperMock.Setup(u => u.RouteUrl(It.IsAny<UrlRouteContext>())).Returns((string?)null);

        var context = new ClientModelValidationContext(actionContext)
        {
            ModelMetadata = modelMetadataMock.Object,
            ValidationMetadataProvider = validationMetadataProviderMock.Object
        };

        var attribute = new TestableRemoteAttribute();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(context));
        Assert.Contains("No URL", ex.Message);
    }
}

public class TestableRemoteAttribute : RemoteAttribute
{
    public new string GetUrl(ClientModelValidationContext context) => base.GetUrl(context);
}
