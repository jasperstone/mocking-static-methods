using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ThrowsOnNullModelId()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, "key", "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ThrowsOnNullBearerKey()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ThrowsOnNullLocation()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", null!, "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ThrowsOnNullProjectId()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "location", null!));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_SucceedsAndRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>(null);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_ThrowsOnNullServices()
    {
        // Arrange
        IServiceCollection services = null!;
        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_ThrowsOnNullTokenProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", (Func<ValueTask<string>>?)null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_SucceedsAndRegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");

        // Act
        var result = services.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", "project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>(null);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project");

        // Assert
        Assert.Same(services, result);
    }
}
