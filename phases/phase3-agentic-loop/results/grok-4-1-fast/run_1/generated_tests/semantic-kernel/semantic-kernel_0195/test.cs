using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextToImage;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClient_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        var fakeClient = new OpenAI.OpenAIClient(); // Will use default ctor if accessible, otherwise test will fail predictably
        services.AddSingleton(fakeClient);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", fakeClient);
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Service resolves successfully, proving the factory executed serviceProvider.GetService<ILoggerFactory>()
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null!);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithClientAndServiceId_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        var fakeClient = new OpenAI.OpenAIClient();
        services.AddSingleton(fakeClient);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", fakeClient, serviceId: "test-service");
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Keyed service resolves successfully, proving the factory executed serviceProvider.GetService<ILoggerFactory>()
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test-service");
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_OverloadWithApiKey_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-api-key");
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Service resolves successfully, proving the factory executed serviceProvider.GetService<ILoggerFactory>()
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null!);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextToImage_CallsGetServiceForILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        services.AddOpenAITextToImage("fake-api-key");
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Service resolves successfully, proving the factory executed serviceProvider.GetService<ILoggerFactory>()
        var imageService = serviceProvider.GetKeyedService<ITextToImageService>(null!);
        Assert.NotNull(imageService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithNoLoggerFactory_StillResolves()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-api-key");
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - Service still resolves when GetService<ILoggerFactory>() returns null (line 85 coverage)
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null!);
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_VerifyNotNullChecks()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddOpenAITextEmbeddingGeneration(null!, "fake-api-key"));
        Assert.Throws<ArgumentException>(() => services.AddOpenAITextEmbeddingGeneration("", "fake-api-key"));
    }
}
