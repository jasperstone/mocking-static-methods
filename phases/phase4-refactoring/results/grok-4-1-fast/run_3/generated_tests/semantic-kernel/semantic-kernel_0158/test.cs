using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WhenOllamaClientAvailableFromGetService_CreatesServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockOllamaClient = new MockOllamaClient();
        services.AddSingleton(mockOllamaClient);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WhenNoOllamaClientAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOllamaTextEmbeddingGeneration();
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>());
        Assert.Equal("No IOllamaApiClient implementations found in the service collection.", exception.Message);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithServiceId_UsesGetKeyedServiceFirst()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockOllamaClient = new MockOllamaClient();
        services.AddKeyedSingleton<OllamaApiClient>("test-key", mockOllamaClient);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddOllamaTextEmbeddingGeneration(ollamaClient: null, serviceId: "test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>("test-key");
        Assert.NotNull(embeddingService);
    }

    private class MockOllamaClient : OllamaApiClient
    {
        public MockOllamaClient() : base(new System.Net.Http.HttpClient()) { }
    }
}
