using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.Extensions.UnitTests;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_FallsBackToGetServiceOllamaApiClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var ollamaClient = new MockOllamaApiClient();
        services.AddSingleton(ollamaClient);

        // Act
        var updatedServices = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaClient: null);

        // Assert
        Assert.Same(services, updatedServices);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_FallsBackToGetServiceIOllamaApiClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var ollamaClient = new MockOllamaApiClient();
        services.AddSingleton<IOllamaApiClient>(ollamaClient);

        // Act
        var updatedServices = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaClient: null);

        // Assert
        Assert.Same(services, updatedServices);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithKeyedOllamaClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var ollamaClient = new MockOllamaApiClient();
        services.AddKeyedSingleton<OllamaApiClient>("test", ollamaClient);

        // Act
        var updatedServices = OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaClient: null, serviceId: "test");

        // Assert
        Assert.Same(services, updatedServices);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test");
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_NoOllamaClientAvailable_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(new LoggerFactory());

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => 
            OllamaServiceCollectionExtensions.AddOllamaTextEmbeddingGeneration(services, ollamaClient: null));
        Assert.Equal("No IOllamaApiClient implementations found in the service collection.", ex.Message);
    }

    private sealed class MockOllamaApiClient : OllamaApiClient, IOllamaApiClient
    {
        public MockOllamaApiClient() : base("http://localhost:11434", "test-model") { }
    }
}
