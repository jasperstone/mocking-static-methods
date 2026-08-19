using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.TextToImage;
using Xunit;
using OpenAI;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    private sealed class FakeOpenAIClient : OpenAIClient
    {
        public FakeOpenAIClient() : base(new OpenAIAuthentication("fake")) { }
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithApiKey_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-api-key");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_Succeeds()
    {
        // Arrange
        var fakeClient = new FakeOpenAIClient();
        var services = new ServiceCollection();
        services.AddSingleton(fakeClient);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAIClient: fakeClient);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithServiceId_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-api-key", serviceId: "test-service");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test-service");
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithDimensions_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", "fake-api-key", dimensions: 1536);

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextToImage_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOpenAITextToImage("fake-api-key");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var imageService = serviceProvider.GetKeyedService<ITextToImageService>(null);
        Assert.NotNull(imageService);
    }
}
