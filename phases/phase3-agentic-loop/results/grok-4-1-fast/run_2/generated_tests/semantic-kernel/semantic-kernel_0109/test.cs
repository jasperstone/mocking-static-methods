using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_RegistersServiceDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("fake-token"),
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(s => s.ServiceType == typeof(object)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "fake-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        var descriptor = Assert.Single(services.Where(s => s.ServiceType == typeof(object)));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithServiceId_RegistersKeyedServiceDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();
        const string serviceId = "test-service";

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "fake-key",
            location: "us-central1",
            projectId: "test-project",
            serviceId: serviceId);

        // Assert
        var descriptor = Assert.Single(services);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ((IServiceCollection)null!).AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullModelId_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddVertexAIEmbeddingGenerator(
            modelId: null!,
            bearerTokenProvider: () => new ValueTask<string>("token"),
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullBearerKey_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: null!,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullLocation_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "key",
            location: null!,
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullProjectId_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "key",
            location: "us-central1",
            projectId: null!));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullTokenProvider_ThrowsArgumentException()
    {
        var services = new ServiceCollection();
        Assert.ThrowsAny<ArgumentException>(() => services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: null!,
            location: "us-central1",
            projectId: "test-project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_MultipleCalls_AddsMultipleDescriptors()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVertexAIEmbeddingGenerator("model1", "key1", "loc1", "proj1");
        services.AddVertexAIEmbeddingGenerator("model2", "key2", "loc2", "proj2");

        // Assert
        Assert.Equal(2, services.Count());
    }
}
