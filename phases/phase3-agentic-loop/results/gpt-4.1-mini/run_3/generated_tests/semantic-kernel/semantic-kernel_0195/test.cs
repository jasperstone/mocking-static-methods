using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.OpenAI.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_CreatesServiceWithLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        // Add a mock ILoggerFactory to the service collection to be returned by GetService
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        services.AddOpenAITextEmbeddingGeneration(
            modelId: "test-model",
            openAIClient: mockOpenAIClient.Object,
            serviceId: "test-service",
            dimensions: 123);

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the ITextEmbeddingGenerationService to trigger the factory delegate
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
        Assert.IsType<OpenAITextEmbeddingGenerationService>(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithoutOpenAIClient_UsesServiceProviderToGetRequiredServiceAndGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        // Add OpenAIClient and ILoggerFactory to the service collection
        services.AddSingleton(mockOpenAIClient.Object);
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        services.AddOpenAITextEmbeddingGeneration(
            modelId: "test-model",
            openAIClient: null,
            serviceId: "test-service",
            dimensions: 456);

        // Build the service provider
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the ITextEmbeddingGenerationService to trigger the factory delegate
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
        Assert.IsType<OpenAITextEmbeddingGenerationService>(embeddingService);
    }
}
