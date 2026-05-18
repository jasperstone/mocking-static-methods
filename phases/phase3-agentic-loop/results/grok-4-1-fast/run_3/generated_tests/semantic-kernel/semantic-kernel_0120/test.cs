using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.ImageToText;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.ChatCompletion;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceTextEmbeddingGeneration("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithEndpoint_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceTextEmbeddingGeneration(new Uri("https://example.com"));

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceTextGeneration_WithModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceTextGeneration("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<ITextGenerationService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_WithModel_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceChatCompletion("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<IChatCompletionService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceImageToText_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceImageToText("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<IImageToTextService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((ServiceCollection?)null)!.AddHuggingFaceTextEmbeddingGeneration("test-model"));
    }
}
