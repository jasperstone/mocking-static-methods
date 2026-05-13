using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

public class KernelTests
{
    [Fact]
    public void ServiceSelector_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var serviceProvider = new ServiceCollection();
        var mockSelector = new Mock<IAIServiceSelector>();
        serviceProvider.AddSingleton(mockSelector.Object);
        var services = serviceProvider.BuildServiceProvider();

        var kernel = new Kernel(services);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.Equal(mockSelector.Object, selector);
    }

    [Fact]
    public void ServiceSelector_ReturnsOrderedAIServiceSelectorInstance_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        var kernel = new Kernel(services);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(selector);
        Assert.IsType<OrderedAIServiceSelector>(selector);
    }

    [Fact]
    public void ServiceSelector_ReturnsSameInstanceAcrossMultipleAccesses()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        var kernel = new Kernel(services);

        // Act
        var selector1 = kernel.ServiceSelector;
        var selector2 = kernel.ServiceSelector;

        // Assert
        Assert.Same(selector1, selector2);
        Assert.IsType<OrderedAIServiceSelector>(selector1);
    }

    [Fact]
    public void LoggerFactory_ReturnsRegisteredLoggerFactory_WhenAvailable()
    {
        // Arrange
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var serviceProvider = new ServiceCollection();
        serviceProvider.AddSingleton(mockLoggerFactory.Object);
        var services = serviceProvider.BuildServiceProvider();

        var kernel = new Kernel(services);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.Equal(mockLoggerFactory.Object, loggerFactory);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactoryInstance_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection().BuildServiceProvider();

        var kernel = new Kernel(services);

        // Act
        var loggerFactory = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, loggerFactory);
    }

    [Fact]
    public void Constructor_UsesEmptyServiceProvider_WhenServicesNull()
    {
        // Act
        var kernel = new Kernel(services: null);

        // Assert
        Assert.Same(EmptyServiceProvider.Instance, kernel.Services);
    }
}
