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
    public void LoggerFactory_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactoryMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Equal(loggerFactoryMock.Object, result);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Equal(NullLoggerFactory.Instance, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var selectorMock = new Mock<IAIServiceSelector>();
        var services = new ServiceCollection();
        services.AddSingleton<IAIServiceSelector>(selectorMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Equal(selectorMock.Object, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsOrderedAIServiceSelector_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Equal(OrderedAIServiceSelector.Instance, result);
    }

    [Fact]
    public void ServiceSelector_GetServiceExtensionCalled_WhenNoServiceRegistered()
    {
        // Arrange - Use Mock<IServiceProvider> to verify GetService was called
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IAIServiceSelector)))
            .Returns((IAIServiceSelector)null);

        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        _ = kernel.ServiceSelector;

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IAIServiceSelector)), Times.Once());
    }

    [Fact]
    public void LoggerFactory_GetServiceExtensionCalled_WhenNoServiceRegistered()
    {
        // Arrange
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns((ILoggerFactory)null);

        var kernel = new Kernel(serviceProviderMock.Object);

        // Act
        _ = kernel.LoggerFactory;

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.Once());
    }

    [Fact]
    public void Culture_DefaultsToInvariantCulture_WhenNotSet()
    {
        // Arrange
        var kernel = new Kernel();

        // Act & Assert
        Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
    }

    [Fact]
    public void Culture_CanBeSetToNull_UsesInvariantCulture()
    {
        // Arrange
        var kernel = new Kernel();

        // Act
        kernel.Culture = null;

        // Assert
        Assert.Equal(CultureInfo.InvariantCulture, kernel.Culture);
    }

    [Fact]
    public void Culture_CanBeSetToCustomCulture()
    {
        // Arrange
        var kernel = new Kernel();
        var customCulture = new CultureInfo("fr-FR");

        // Act
        kernel.Culture = customCulture;

        // Assert
        Assert.Equal(customCulture, kernel.Culture);
    }
}
