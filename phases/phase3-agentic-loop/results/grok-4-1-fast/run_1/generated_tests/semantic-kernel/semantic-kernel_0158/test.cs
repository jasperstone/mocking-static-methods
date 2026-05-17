using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using OllamaSharp;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithNonKeyedOllamaApiClient_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "test-model");
        services.AddSingleton(ollamaClient);

        // Act
        services.AddOllamaTextEmbeddingGeneration();

        // Assert - Should resolve without exception, proving GetService<OllamaApiClient>() on line 344 works
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithIOllamaApiClientFallback_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"), "test-model");
        services.AddSingleton<IOllamaApiClient>(ollamaClient);

        // Act
        services.AddOllamaTextEmbeddingGeneration();

        // Assert - Should resolve via GetRequiredService<IOllamaApiClient>() as OllamaApiClient fallback
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithNoClient_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOllamaTextEmbeddingGeneration();

        // Assert - All GetService/GetKeyedService/GetRequiredService paths fail, throws exception
        var serviceProvider = services.BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ITextEmbeddingGenerationService>());
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();

        // Assert
        Assert.Same(services, result);
    }
}
