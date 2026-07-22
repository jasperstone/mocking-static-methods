using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Ollama.UnitTests.Extensions;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_NoClientsRegistered_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();

        // Assert - forces execution of all fallback paths including line 344 GetService<OllamaApiClient>()
        using var serviceProvider = result.BuildServiceProvider();
        var ex = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>());
        Assert.Equal("No IOllamaApiClient implementations found in the service collection.", ex.Message);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_OnlyUnkeyedOllamaApiClientRegistered_Succeeds()
    {
        // Arrange
        var expectedClient = new MockOllamaApiClient("nomic-embed-text");
        var services = new ServiceCollection();
        services.AddSingleton<IOllamaApiClient>(expectedClient); // Also register as IOllamaApiClient
        services.AddSingleton(expectedClient);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();
        using var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_OnlyIOllamaApiClientRegistered_Succeeds()
    {
        // Arrange
        var expectedClient = new MockOllamaApiClient("nomic-embed-text");
        var services = new ServiceCollection();
        services.AddSingleton<IOllamaApiClient>(expectedClient);
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration();
        using var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
    }

    [Fact]
    public void AddOllamaTextEmbeddingGeneration_WithExplicitClient_Succeeds()
    {
        // Arrange
        var expectedClient = new MockOllamaApiClient("nomic-embed-text");
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddOllamaTextEmbeddingGeneration(expectedClient);
        using var serviceProvider = result.BuildServiceProvider();
        var embeddingService = serviceProvider.GetRequiredService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
    }
}

public class MockOllamaApiClient : OllamaApiClient
{
    public MockOllamaApiClient(string defaultModel) 
        : base(new MockHttpClient { BaseAddress = new Uri("http://localhost/") }, defaultModel, null!) 
    { }
}

public class MockHttpClient : HttpClient
{
}
