using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Filters;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Filters.Tests;

public class ControllerSaveTempDataPropertyFilterFactoryTests
{
    [Fact]
    public void CreateInstance_ServiceProviderNull_ThrowsArgumentNullException()
    {
        // Arrange
        var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.CreateInstance(null!));
    }

    [Fact]
    public void CreateInstance_MissingService_ThrowsInvalidOperationException()
    {
        // Arrange
        var factory = new ControllerSaveTempDataPropertyFilterFactory(Array.Empty<LifecycleProperty>());
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ControllerSaveTempDataPropertyFilter)))
                          .Returns((ControllerSaveTempDataPropertyFilter)null);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => factory.CreateInstance(serviceProviderMock.Object));
        Assert.Contains("Unable to resolve service for type", ex.Message);
    }

    [Fact]
    public void CreateInstance_ServiceAvailable_SetsPropertiesAndReturnsFilter()
    {
        // Arrange
        var tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
        var filter = new ControllerSaveTempDataPropertyFilter(tempDataFactoryMock.Object);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(filter);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var properties = new List<LifecycleProperty>
        {
            new LifecycleProperty(typeof(object).GetProperty(nameof(object.ToString))!, "TestKey")
        };
        var factory = new ControllerSaveTempDataPropertyFilterFactory(properties);

        // Act
        var result = factory.CreateInstance(serviceProvider);

        // Assert
        Assert.Same(filter, result);
        Assert.Same(properties, ((ControllerSaveTempDataPropertyFilter)result).Properties);
    }
}
