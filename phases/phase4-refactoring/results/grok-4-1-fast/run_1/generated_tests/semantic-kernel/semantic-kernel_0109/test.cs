using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services!.AddVertexAIEmbeddingGenerator("model", bearerTokenProvider, "location", "project"));
        Assert.Equal("services", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, bearerTokenProvider, "location", "project"));
        Assert.Equal("modelId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerTokenProvider", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", bearerTokenProvider, null!, "project"));
        Assert.Equal("location", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullProjectId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", bearerTokenProvider, "location", null!));
        Assert.Equal("projectId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        var bearerTokenProvider = () => new ValueTask<string>("token");

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Null(descriptor.ServiceKey);

        var provider = services.BuildServiceProvider();
        var embeddingGenerator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
        Assert.NotNull(embeddingGenerator);
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
        var exception = Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "location", null!));
        Assert.Equal("projectId", exception.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        Assert.NotEmpty(services);

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.Null(descriptor.ServiceKey);

        var provider = services.BuildServiceProvider();
        var embeddingGenerator = provider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithCustomServiceId_RegistersWithCorrectKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(NullLoggerFactory.Instance);
        var bearerTokenProvider = () => new ValueTask<string>("token");

        const string serviceId = "custom-service";

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: bearerTokenProvider,
            location: "us-central1",
            projectId: "test-project",
            serviceId: serviceId);

        // Assert
        var provider = services.BuildServiceProvider();
        var embeddingGenerator = provider.GetKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
        Assert.NotNull(embeddingGenerator);
    }
}
