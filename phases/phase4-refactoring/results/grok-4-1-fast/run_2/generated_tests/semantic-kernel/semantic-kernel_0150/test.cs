using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaChatCompletion_EndpointOverload_ResolvesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        string modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act
        services.AddOllamaChatCompletion(modelId, endpoint);
        using var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetKeyedService<IChatCompletionService>(null);

        // Assert - Verify logger factory was resolved via GetService
        mockLoggerFactory.Verify(f => f, Times.Once);
    }

    [Fact]
    public void AddOllamaChatCompletion_HttpClientOverload_ResolvesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        string modelId = "test-model";
        using var httpClient = new HttpClient();

        // Act
        services.AddOllamaChatCompletion(modelId, httpClient);
        using var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetKeyedService<IChatCompletionService>(null);

        // Assert - Verify logger factory was resolved via GetService
        mockLoggerFactory.Verify(f => f, Times.Once);
    }

    [Fact]
    public void AddOllamaTextGeneration_EndpointOverload_ResolvesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        string modelId = "test-model";
        var endpoint = new Uri("http://localhost:11434");

        // Act
        services.AddOllamaTextGeneration(modelId, endpoint);
        using var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetKeyedService<ITextGenerationService>(null);

        // Assert - Verify logger factory was resolved via GetService
        mockLoggerFactory.Verify(f => f, Times.Once);
    }

    [Fact]
    public void AddOllamaTextGeneration_HttpClientOverload_ResolvesLoggerFactoryFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        services.AddSingleton(mockLoggerFactory.Object);

        string modelId = "test-model";
        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434/") };

        // Act
        services.AddOllamaTextGeneration(modelId, httpClient);
        using var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetKeyedService<ITextGenerationService>(null);

        // Assert - Verify logger factory was resolved via GetService
        mockLoggerFactory.Verify(f => f, Times.Once);
    }
}
