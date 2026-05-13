using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests;

public class ControllerSaveTempDataPropertyFilterFactoryTests
{
    [Fact]
    public void CreateInstance_ThrowsArgumentNullException_WhenServiceProviderIsNull()
    {
        // Arrange
        var properties = new List<LifecycleProperty>();
        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null!));
    }

    [Fact]
    public void CreateInstance_ReturnsFilter_WhenServiceProviderHasRequiredService()
    {
        // Arrange
        var properties = new List<LifecycleProperty> { new LifecycleProperty() };
        var mockFilter = new Mock<ControllerSaveTempDataPropertyFilter>(Mock.Of<ITempDataDictionaryFactory>()).Object;
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(sp => sp.GetRequiredService<ControllerSaveTempDataPropertyFilter>())
            .Returns(mockFilter)
            .Verifiable();

        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act
        var result = factory.CreateInstance(serviceProvider.Object);

        // Assert
        Assert.Same(mockFilter, result);
        Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
        serviceProvider.Verify();
    }

    [Fact]
    public void IsReusable_ReturnsFalse()
    {
        // Arrange
        var properties = new List<LifecycleProperty>();
        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act & Assert
        Assert.False(factory.IsReusable);
    }
}
