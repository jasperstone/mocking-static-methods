using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var tokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services!.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", "project"));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var tokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, tokenProvider, "location", "project"));
        Assert.Equal("modelId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullTokenProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerTokenProvider", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var tokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", tokenProvider, null!, "project"));
        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullProjectId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var tokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", null!));
        Assert.Equal("projectId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_ValidParameters_AddsKeyedSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var tokenProvider = new Func<ValueTask<string>>(() => new ValueTask<string>("token"));

        // Act
        var result = services.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", "project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var embeddingService = services.FirstOrDefault(s => s.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
        Assert.NotNull(embeddingService);
        Assert.Equal(ServiceLifetime.Singleton, embeddingService.Lifetime);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services!.AddVertexAIEmbeddingGenerator("model", "key", "location", "project"));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, "key", "location", "project"));
        Assert.Equal("modelId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullBearerKey_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerKey", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", null!, "project"));
        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullProjectId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("method", "key", "location", null!));
        Assert.Equal("projectId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ValidParameters_AddsKeyedSingletonService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var embeddingService = services.FirstOrDefault(s => s.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
        Assert.NotNull(embeddingService);
        Assert.Equal(ServiceLifetime.Singleton, embeddingService.Lifetime);
    }
}
