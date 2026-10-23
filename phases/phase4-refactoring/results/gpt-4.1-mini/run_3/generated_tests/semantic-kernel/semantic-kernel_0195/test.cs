using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using OpenAI;
using Xunit;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesServiceProviderGetServiceForLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockOpenAIClient = new Mock<OpenAIClient>();

        // Register OpenAIClient and ILoggerFactory in the service provider
        services.AddSingleton(mockOpenAIClient.Object);
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var result = Microsoft.SemanticKernel.OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(
            services,
            modelId: "test-model",
            openAIClient: mockOpenAIClient.Object,
            serviceId: "test-service",
            dimensions: 123);

        // Build the service provider to resolve the factory delegate
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the ITextEmbeddingGenerationService from the service provider
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithNullOpenAIClient_ResolvesOpenAIClientFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockOpenAIClient = new Mock<OpenAIClient>();
        var mockLoggerFactory = new Mock<ILoggerFactory>();

        // Register OpenAIClient and ILoggerFactory in the service provider
        services.AddSingleton(mockOpenAIClient.Object);
        services.AddSingleton(mockLoggerFactory.Object);

        // Act
        var result = Microsoft.SemanticKernel.OpenAIServiceCollectionExtensions.AddOpenAITextEmbeddingGeneration(
            services,
            modelId: "test-model",
            openAIClient: null,
            serviceId: "test-service",
            dimensions: 123);

        // Build the service provider to resolve the factory delegate
        var serviceProvider = services.BuildServiceProvider();

        // Resolve the ITextEmbeddingGenerationService from the service provider
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(embeddingService);
    }
}
