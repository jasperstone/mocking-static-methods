using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.SemanticKernel.Tests;

public class KernelTests
{
    [Fact]
    public void LoggerFactory_ReturnsService_WhenAvailable()
    {
        // Arrange
        var loggerFactory = NullLoggerFactory.Instance;
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactory, result);
    }

    [Fact]
    public void LoggerFactory_ReturnsNullLoggerFactory_WhenNotAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(NullLoggerFactory.Instance, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsService_WhenAvailable()
    {
        // Arrange
        var serviceSelector = new MockIAIServiceSelector();
        var services = new ServiceCollection();
        services.AddSingleton<IAIServiceSelector>(serviceSelector);
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.Same(serviceSelector, result);
    }

    [Fact]
    public void ServiceSelector_ReturnsFallback_WhenNotAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IAIServiceSelector>(result);
    }

    private class MockIAIServiceSelector : IAIServiceSelector
    {
        public bool TrySelectAIService<T>(
            Kernel kernel,
            KernelFunction function,
            KernelArguments arguments,
            [NotNullWhen(true)] out T? service,
            out PromptExecutionSettings? serviceSettings) where T : class, IAIService
        {
            service = null!;
            serviceSettings = null;
            return false;
        }
    }
}
