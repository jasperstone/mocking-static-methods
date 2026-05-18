using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("token"), "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullModelId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, () => new ValueTask<string>("token"), "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullLocation_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("token"), null!, "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_NullProjectId_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("token"), "location", null!));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersKeyedServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "us-central1",
            projectId: "test-project");

        Assert.Same(services, result);

        // Assert - Verify the GetService<ILoggerFactory> call happens during factory invocation (line 60)
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredKeyedService<object>(null);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_CustomServiceId_RegistersKeyedServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        const string customServiceId = "custom-service";

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "us-central1",
            projectId: "test-project",
            serviceId: customServiceId);

        Assert.Same(services, result);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredKeyedService<object>(customServiceId);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "token", "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullBearerKey_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersKeyedServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-token",
            location: "us-central1",
            projectId: "test-project");

        Assert.Same(services, result);

        // Assert - Verify the GetService<ILoggerFactory> call happens during factory invocation
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredKeyedService<object>(null);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_CustomServiceId_RegistersKeyedServiceCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        const string customServiceId = "custom-service";

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-token",
            location: "us-central1",
            projectId: "test-project",
            serviceId: customServiceId);

        Assert.Same(services, result);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetRequiredKeyedService<object>(customServiceId);
        Assert.NotNull(embeddingGenerator);
    }
}
