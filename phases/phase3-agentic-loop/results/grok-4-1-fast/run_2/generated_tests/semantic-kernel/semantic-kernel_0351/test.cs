using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests;

public class KernelTests
{
    [Fact]
    public void LoggerFactoryProperty_ReturnsService_WhenAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = NullLoggerFactory.Instance;
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.LoggerFactory;

        // Assert
        Assert.Same(loggerFactory, result);
    }

    [Fact]
    public void LoggerFactoryProperty_ReturnsNullLoggerFactory_WhenNotAvailable()
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
    public void ServiceSelectorProperty_ReturnsService_WhenAvailable()
    {
        // Arrange
        var mockSelector = new MockIAIServiceSelector();
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
    public void ServiceSelectorProperty_ReturnsOrderedAIServiceSelector_WhenNotAvailable()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = false, ValidateOnBuild = false });
        var kernel = new Kernel(serviceProvider);

        // Act
        var result = kernel.ServiceSelector;

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IAIServiceSelector>(result);
    }

    private sealed class MockIAIServiceSelector : IAIServiceSelector
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
