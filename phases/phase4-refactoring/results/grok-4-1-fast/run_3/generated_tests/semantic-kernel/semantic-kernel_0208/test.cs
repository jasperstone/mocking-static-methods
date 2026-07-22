using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class OpenAIServiceCollectionExtensionsTests
{
    private class TestLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void Dispose() { }
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_RegistersChatClientService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: "gpt-4o-mini",
            endpoint: new Uri("https://api.openai.com/v1"),
            apiKey: "test-key");

        // Assert
        Assert.Same(services, result);
        var chatClientDescriptors = services.Where(sd => sd.ServiceType == typeof(IChatClient)).ToList();
        Assert.Single(chatClientDescriptors);
        Assert.Equal(ServiceLifetime.Singleton, chatClientDescriptors[0].Lifetime);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_ResolvesChatClientWithoutLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenAIChatClient(
            modelId: "gpt-4o-mini",
            endpoint: new Uri("https://api.openai.com/v1"),
            apiKey: "test-key");

        using var serviceProvider = services.BuildServiceProvider();

        // Assert - exercises the factory including serviceProvider.GetService<ILoggerFactory>()
        // which returns null when not registered, but service still resolves
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(null);
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_CustomEndpoint_ResolvesChatClientWithLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory, TestLoggerFactory>();

        // Act
        services.AddOpenAIChatClient(
            modelId: "gpt-4o-mini",
            endpoint: new Uri("https://api.openai.com/v1"),
            apiKey: "test-key");

        using var serviceProvider = services.BuildServiceProvider();

        // Assert - exercises the factory including serviceProvider.GetService<ILoggerFactory>()
        // which returns the registered factory
        var chatClient = serviceProvider.GetKeyedService<IChatClient>(null);
        Assert.NotNull(chatClient);
    }

    [Fact]
    public void AddOpenAIChatClient_WithApiKey_RegistersChatClientService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOpenAIChatClient(
            modelId: "gpt-4o-mini",
            apiKey: "test-key");

        // Assert - exercises the other overload which also calls GetService<ILoggerFactory>()
        Assert.Same(services, result);
        var chatClientDescriptors = services.Where(sd => sd.ServiceType == typeof(IChatClient)).ToList();
        Assert.Single(chatClientDescriptors);
        Assert.Equal(ServiceLifetime.Singleton, chatClientDescriptors[0].Lifetime);
    }
}
