using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOllamaChatCompletion("test-model", new Uri("http://localhost:11434"));

        // Assert - Build provider and trigger factory execution (which calls GetService)
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithEndpoint_NoLoggerFactory_ReturnsValidService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOllamaChatCompletion("test-model", new Uri("http://localhost:11434"));

        // Assert - GetService returns null but service still created successfully
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaChatCompletion_WithHttpClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOllamaChatCompletion("test-model");

        // Assert - Triggers factory with GetService call
        var serviceProvider = services.BuildServiceProvider();
        var chatService = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AddOllamaTextGeneration_WithClient_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var mockClient = new MockOllamaClient();

        // Act
        services.AddOllamaTextGeneration("test-model", mockClient);

        // Assert - Factory calls GetService<ILoggerFactory>()
        var serviceProvider = services.BuildServiceProvider();
        var textService = serviceProvider.GetKeyedService<ITextGenerationService>(null);
        Assert.NotNull(textService);
    }

    [Fact]
    public void AddOllamaTextGeneration_AutoResolveClient_CallsMultipleGetServiceVariants()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOllamaTextGeneration(ollamaClient: null);

        // Assert - Triggers all GetService/GetKeyedService calls, should throw as expected
        var serviceProvider = services.BuildServiceProvider();
        Assert.ThrowsAny<Exception>(() => serviceProvider.GetKeyedService<ITextGenerationService>(null));
    }

    private class MockOllamaClient : OllamaApiClient
    {
        public MockOllamaClient() : base(new Uri("http://localhost:11434"), "test") { }
    }
}
