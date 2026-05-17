using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Xunit;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.SemanticKernel.UnitTests;

public class KernelServiceExtensionsTests
{
    [Fact]
    public void LoggerFactoryProperty_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var loggerFactory = NullLoggerFactory.Instance;
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactory, result);
    }

    [Fact]
    public void LoggerFactoryProperty_ReturnsNullLoggerFactory_WhenNotRegistered()
    {
        // Arrange
        var kernel = new Kernel();

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, result);
    }

    [Fact]
    public void ServiceSelectorProperty_ReturnsRegisteredService_WhenAvailable()
    {
        // Arrange
        var mockSelector = new MockAIServiceSelector();
        var services = new ServiceCollection();
        services.AddSingleton<IAIServiceSelector>(mockSelector);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Same(mockSelector, result);
    }

    [Fact]
    public void ServiceSelectorProperty_ReturnsFallback_WhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void ServiceSelectorProperty_CallsGetService_OnIServiceProvider()
    {
        // Arrange
        var mockProvider = new MockServiceProvider();
        var kernel = new Kernel(mockProvider);

        // Act
        _ = kernel.ServiceSelector;

        // Assert - Verifies the GetService extension method was called (line 192)
        Assert.True(mockProvider.GetServiceCalled);
    }

    private sealed class MockServiceProvider : IServiceProvider
    {
        public bool GetServiceCalled { get; private set; }

        public object? GetService(Type serviceType)
        {
            GetServiceCalled = true;
            return serviceType == typeof(IAIServiceSelector) ? new MockAIServiceSelector() : null;
        }
    }

    private sealed class MockAIServiceSelector : IAIServiceSelector
    {
        public bool TrySelectAIService<T>(
            Kernel kernel,
            KernelFunction function,
            KernelArguments arguments,
            [NotNullWhen(true)] out T? service,
            out PromptExecutionSettings? serviceSettings) where T : class, IAIService
        {
            service = default;
            serviceSettings = null;
            return false;
        }
    }
}
