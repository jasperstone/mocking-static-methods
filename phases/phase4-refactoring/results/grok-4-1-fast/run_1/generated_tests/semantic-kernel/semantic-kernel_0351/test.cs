using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class KernelServicePropertiesTests
{
    [Fact]
    public void ServiceSelector_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var mockSelector = new Mock<IAIServiceSelector>().Object;
        var services = new ServiceCollection();
        services.AddSingleton<IAIServiceSelector>(mockSelector);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
        var kernel = new Kernel(serviceProvider);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.Same(mockSelector, selector);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultInstance_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
        var kernel = new Kernel(serviceProvider);

        // Act
        var selector = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(selector);
    }

    [Fact]
    public void LoggerFactory_ReturnsRegisteredFactory_WhenAvailable()
    {
        // Arrange
        var mockFactory = new Mock<ILoggerFactory>().Object;
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(mockFactory);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
        var kernel = new Kernel(serviceProvider);

        // Act
        var factory = kernel.LoggerFactory;

        // Assert
        Assert.Same(mockFactory, factory);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
        var kernel = new Kernel(serviceProvider);

        // Act
        var factory = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, factory);
    }

    [Fact]
    public void Constructor_UsesEmptyServiceProvider_WhenServicesNull()
    {
        // Act
        var kernel = new Kernel(services: null);

        // Assert
        Assert.NotNull(kernel.Services);
    }
}
