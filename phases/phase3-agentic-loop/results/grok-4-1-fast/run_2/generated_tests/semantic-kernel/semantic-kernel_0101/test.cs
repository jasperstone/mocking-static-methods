using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Google.Tests.Extensions;

public class VertexAIKernelBuilderExtensionsTests
{
    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithTokenProvider_RegistersServiceWithGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MockKernelBuilder(services);
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());

        // Act
        builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerTokenProvider: async () => "token",
            location: "us-central1",
            projectId: "test-project");

        // Assert - Verify service was registered (GetService called during factory execution)
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var servicesRegistered = scope.ServiceProvider.GetServices<IChatCompletionService>();
        Assert.Single(servicesRegistered);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithBearerKey_RegistersServiceWithGetServiceCall()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MockKernelBuilder(services);
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());

        // Act
        builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: "bearer-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var servicesRegistered = scope.ServiceProvider.GetServices<IChatCompletionService>();
        Assert.Single(servicesRegistered);
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullBuilder_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IKernelBuilder)null!).AddVertexAIGeminiChatCompletion("model", async () => "token", "loc", "proj"));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_NullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MockKernelBuilder(services);
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion(null!, async () => "token", "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion("model", null!, "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion("model", "key", null!, "proj"));
        Assert.Throws<ArgumentNullException>(() => builder.AddVertexAIGeminiChatCompletion("model", "key", "loc", null!));
    }

    [Fact]
    public void AddVertexAIGeminiChatCompletion_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new MockKernelBuilder(services);
        services.AddSingleton<ILoggerFactory>(new MockLoggerFactory());

        // Act
        builder.AddVertexAIGeminiChatCompletion(
            modelId: "gemini-pro",
            bearerKey: "key",
            location: "us-central1",
            projectId: "test-project",
            serviceId: "test-service");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetKeyedService<IChatCompletionService>("test-service");
        Assert.NotNull(service);
    }
}

// Mock implementations
public class MockKernelBuilder : IKernelBuilder
{
    public IServiceCollection Services { get; }
    public IKernelBuilderPlugins Plugins => new MockKernelBuilderPlugins(Services);

    public MockKernelBuilder(IServiceCollection services)
    {
        Services = services;
    }
}

public class MockKernelBuilderPlugins : IKernelBuilderPlugins
{
    public IServiceCollection Services { get; }

    public MockKernelBuilderPlugins(IServiceCollection services)
    {
        Services = services;
    }
}

public class MockLoggerFactory : ILoggerFactory
{
    public void AddProvider(ILoggerProvider provider) { }
    public ILogger CreateLogger(string categoryName) => new MockLogger();
    public void Dispose() { }
}

public class MockLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
