using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.HuggingFace.UnitTests;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceTextEmbeddingGeneration(
            model: "test-model",
            endpoint: new Uri("https://example.com"),
            serviceId: "test-service");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verifies the factory was called with serviceProvider.GetService<ILoggerFactory>()
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test-service");
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithoutModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceTextEmbeddingGeneration(
            endpoint: new Uri("https://example.com"));

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Covers serviceProvider.GetService<ILoggerFactory>() call
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddHuggingFaceImageToText_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceImageToText(
            model: "test-model",
            endpoint: new Uri("https://example.com"),
            serviceId: "test-service");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Covers serviceProvider.GetService<ILoggerFactory>() call on line ~188
        var imageService = serviceProvider.GetKeyedService<IImageToTextService>("test-service");
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceTextGeneration(
            model: "test-model",
            endpoint: new Uri("https://example.com"));

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var textService = serviceProvider.GetService<ITextGenerationService>();
        Assert.NotNull(textService);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHuggingFaceChatCompletion(
            model: "test-model",
            endpoint: new Uri("https://example.com"));

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var chatService = serviceProvider.GetService<IChatCompletionService>();
        Assert.NotNull(chatService);
    }

    [Fact]
    public void AllHuggingFaceServiceCollectionExtensions_ReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert - Verify fluent interface pattern (covers method chaining)
        Assert.Same(services, services.AddHuggingFaceTextEmbeddingGeneration("model1"));
        Assert.Same(services, services.AddHuggingFaceImageToText("model2"));
        Assert.Same(services, services.AddHuggingFaceTextGeneration("model3"));
        Assert.Same(services, services.AddHuggingFaceChatCompletion("model4"));
    }
}
