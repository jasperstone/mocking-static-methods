using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;

public class KernelTests
{
    [Fact]
    public void LoggerFactory_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var loggerFactory = new NullLoggerFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactory, result);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var mockSelector = new MockAIServiceSelector();
        var services = new ServiceCollection();
        services.AddSingleton<IAIServiceSelector>(mockSelector);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Same(mockSelector, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsDefaultInstance_WhenNoServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(result);
    }

    private sealed class MockAIServiceSelector : IAIServiceSelector
    {
        public bool TrySelectAIService<T>(
            Kernel kernel,
            KernelFunction function,
            KernelArguments arguments,
            out T? service,
            out PromptExecutionSettings? serviceSettings) where T : class, IAIService
        {
            service = null;
            serviceSettings = null;
            return false;
        }
    }
}
