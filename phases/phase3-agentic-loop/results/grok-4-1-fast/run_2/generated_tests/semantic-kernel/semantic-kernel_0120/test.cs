using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
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
    public void AddHuggingFaceTextGeneration_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceTextGeneration("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<Microsoft.SemanticKernel.TextGeneration.ITextGenerationService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceChatCompletion_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddHuggingFaceChatCompletion("test-model");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetKeyedService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>(null);
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
        var service = serviceProvider.GetKeyedService<Microsoft.SemanticKernel.ImageToText.IImageToTextService>(null);
        Assert.NotNull(service);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_NullServices_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((ServiceCollection?)null)!.AddHuggingFaceTextEmbeddingGeneration("test-model"));
    }
}
