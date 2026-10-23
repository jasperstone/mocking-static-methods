using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using OpenAI.Auth;
using OpenAI;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class OpenAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_UsesGetServiceILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);
        var openAIClient = new OpenAIClient(new ApiKeyCredential("fake-key"));

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAIClient);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);

        Assert.NotNull(embeddingService);
        Assert.Same(mockLoggerFactory, GetLoggerFactoryFromService(embeddingService));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClientAndServiceId_UsesGetServiceILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);
        var openAIClient = new OpenAIClient(new ApiKeyCredential("fake-key"));

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAIClient, serviceId: "test-service");

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>("test-service");

        Assert.NotNull(embeddingService);
        Assert.Same(mockLoggerFactory, GetLoggerFactoryFromService(embeddingService));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithOpenAIClient_NoLoggerFactory_ReturnsNullLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var openAIClient = new OpenAIClient(new ApiKeyCredential("fake-key"));

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAIClient);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);

        Assert.NotNull(embeddingService);
        Assert.Null(GetLoggerFactoryFromService(embeddingService));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_ApiKeyVersion_UsesGetServiceILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", "fake-api-key");

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);

        Assert.NotNull(embeddingService);
        Assert.Same(mockLoggerFactory, GetLoggerFactoryFromService(embeddingService));
    }

    [Fact]
    public void AddOpenAITextEmbeddingGeneration_WithDimensions_UsesGetServiceILoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockLoggerFactory = new MockLoggerFactory();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);
        var openAIClient = new OpenAIClient(new ApiKeyCredential("fake-key"));

        // Act
        services.AddOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAIClient, dimensions: 1536);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var embeddingService = serviceProvider.GetKeyedService<ITextEmbeddingGenerationService>(null);

        Assert.NotNull(embeddingService);
        Assert.Same(mockLoggerFactory, GetLoggerFactoryFromService(embeddingService));
    }

    private static ILoggerFactory? GetLoggerFactoryFromService(ITextEmbeddingGenerationService service)
    {
        var field = service.GetType().GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance) 
                   ?? service.GetType().GetField("loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(service) as ILoggerFactory;
    }

    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void Dispose() { }
    }
}
