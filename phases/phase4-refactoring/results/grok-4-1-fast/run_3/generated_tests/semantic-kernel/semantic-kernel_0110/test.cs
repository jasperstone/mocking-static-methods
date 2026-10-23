using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerKey_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerators = serviceProvider.GetServices<IEmbeddingGenerator<string, Embedding<float>>>();
        Assert.Single(embeddingGenerators);
        Assert.NotNull(embeddingGenerators.First());
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerTokenProvider_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "test-location",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerators = serviceProvider.GetServices<IEmbeddingGenerator<string, Embedding<float>>>();
        Assert.Single(embeddingGenerators);
        Assert.NotNull(embeddingGenerators.First());
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerKey_WithServiceId_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        const string serviceId = "test-service";

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project",
            serviceId: serviceId);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(serviceId);
        Assert.NotNull(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            ((IServiceCollection)null!).AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "loc",
                projectId: "proj"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullModelId_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddVertexAIEmbeddingGenerator(
                modelId: null!,
                bearerKey: "key",
                location: "loc",
                projectId: "proj"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullBearerKey_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: null!,
                location: "loc",
                projectId: "proj"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullLocation_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: null!,
                projectId: "proj"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullProjectId_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerKey: "key",
                location: "loc",
                projectId: null!));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullBearerTokenProvider_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => 
            services.AddVertexAIEmbeddingGenerator(
                modelId: "model",
                bearerTokenProvider: null!,
                location: "loc",
                projectId: "proj"));
    }
}
