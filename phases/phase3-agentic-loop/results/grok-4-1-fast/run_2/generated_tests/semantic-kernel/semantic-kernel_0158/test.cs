using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.Tests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithUnkeyedOllamaApiClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockClient = new MockOllamaApiClient();
        services.AddSingleton<OllamaApiClient>(mockClient);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithLoggerFactory_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithServiceIdAndUnkeyedFallback_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var testClient = new MockOllamaApiClient();
        services.AddSingleton<OllamaApiClient>(testClient);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration(serviceId: "test");

        // Assert
        var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test");
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_NoOllamaClientAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var result = services.AddOllamaTextEmbeddingGeneration();
        var serviceProvider = result.BuildServiceProvider();
        
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>());
    }

    private class MockOllamaApiClient : OllamaApiClient
    {
        public MockOllamaApiClient() : base("http://localhost:11434", "test-model") { }
    }
}
