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
    public void AddVertexAIEmbeddingGenerator_StringOverload_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddVertexAIEmbeddingGenerator("model", "key", "location", "project"));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_ThrowsArgumentNullException_WhenModelIdIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, "key", "location", "project"));
        Assert.Equal("modelId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_ThrowsArgumentNullException_WhenBearerKeyIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerKey", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_ThrowsArgumentNullException_WhenLocationIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", null!, "project"));
        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_ThrowsArgumentNullException_WhenProjectIdIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "location", null!));
        Assert.Equal("projectId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_WithValidParameters_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>();
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_StringOverload_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project", serviceId: "test-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>("test-key");
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_TokenProviderOverload_ThrowsArgumentNullException_WhenProviderIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", (Func<ValueTask<string>>?)null!, "location", "project"));
        Assert.Equal("bearerTokenProvider", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_TokenProviderOverload_WithValidParameters_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("key"), "location", "project");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredService<Microsoft.Extensions.AI.IEmbeddingGenerator<string, Microsoft.Extensions.AI.Embedding<float>>>();
        Assert.NotNull(embeddingGenerator);
    }
}
