using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;

public class ControllerSaveTempDataPropertyFilterFactoryTests
{
    [Fact]
    public void CreateInstance_NullServiceProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null));
    }

    [Fact]
    public void CreateInstance_ValidServiceProvider_ReturnsConfiguredFilter()
    {
        // Arrange
        var tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
        var filter = new ControllerSaveTempDataPropertyFilter(tempDataFactoryMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
            .Returns(filter)
            .Verifiable();

        var dummyType = typeof(TestController);
        var propertyInfo = dummyType.GetProperty(nameof(TestController.MyProperty))!;
        var properties = new[] { new LifecycleProperty(propertyInfo, "TestKey") };

        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act
        var result = factory.CreateInstance(serviceProviderMock.Object);

        // Assert
        Assert.Same(filter, result);
        Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
        serviceProviderMock.Verify(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>(), Times.Once);
    }

    [Fact]
    public void IsReusable_ReturnsFalse()
    {
        // Arrange
        var factory = new ControllerSaveTempDataPropertyFactory(Array.Empty<LifecycleProperty>());

        // Act & Assert
        Assert.False(factory.IsReusable);
    }

    private class TestController
    {
        public string MyProperty { get; set; } = string.Empty;
    }
}
