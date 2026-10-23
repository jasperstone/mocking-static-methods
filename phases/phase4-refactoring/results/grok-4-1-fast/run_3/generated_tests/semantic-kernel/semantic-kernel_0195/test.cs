using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ClientOverload_WithLoggerFactory_UsesGetServiceResult()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = new LoggerFactory();
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", (OpenAIClient?)null);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);

        // Verify serviceProvider.GetService<ILoggerFactory>() was called and used
        var loggerField = embeddingService.GetType().GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(loggerField);
        var actualLoggerFactory = loggerField.GetValue(embeddingService);
        Assert.Same(loggerFactory, actualLoggerFactory);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ClientOverload_NoLoggerFactory_UsesNullFromGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", (OpenAIClient?)null);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);

        // Verify serviceProvider.GetService<ILoggerFactory>() returned null
        var loggerField = embeddingService.GetType().GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(loggerField);
        var actualLoggerFactory = loggerField.GetValue(embeddingService);
        Assert.Null(actualLoggerFactory);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ApiKeyOverload_WithLoggerFactory_UsesGetServiceResult()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = new LoggerFactory();
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-key");

        // Assert - covers the other overload that also uses serviceProvider.GetService<ILoggerFactory>()
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);
        Assert.NotNull(embeddingService);

        var loggerField = embeddingService.GetType().GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(loggerField);
        var actualLoggerFactory = loggerField.GetValue(embeddingService);
        Assert.Same(loggerFactory, actualLoggerFactory);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ClientOverload_ValidatesModelId()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - covers Verify.NotNullOrWhiteSpace(modelId)
        Assert.Throws<ArgumentNullException>(() => services.AddOpenAITextEmbeddingGeneration(null!, (OpenAIClient?)null));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ApiKeyOverload_ValidatesModelId()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddOpenAITextEmbeddingGeneration(null!, "fake-key"));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ClientOverload_ValidatesServices()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection?)null)!.AddOpenAITextEmbeddingGeneration("model", (OpenAIClient?)null));
    }
}
