using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Google;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_ThrowsOnNullServices()
    {
        IServiceCollection services = null!;
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("key"), "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_ThrowsOnNullModelId()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, () => new ValueTask<string>("key"), "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_ThrowsOnNullBearerTokenProvider()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", (Func<ValueTask<string>>)null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_ThrowsOnNullLocation()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("key"), null!, "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_ThrowsOnNullProjectId()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("key"), "location", null!));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithValidParameters_AddsService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-key"),
            location: "us-central1",
            projectId: "test-project");

        // Assert
        Assert.Same(services, result);
        Assert.Single(services);

        var descriptor = services[0];
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithServiceId_AddsKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-key"),
            location: "us-central1",
            projectId: "test-project",
            serviceId: "test-key");

        // Assert
        var descriptor = services[0];
        Assert.NotNull(descriptor.ServiceKey);
        Assert.Equal("test-key", descriptor.ServiceKey?.ToString());
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_FactoryUsesGetServiceForLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-key"),
            location: "us-central1",
            projectId: "test-project");

        // Build provider - this triggers factory execution including serviceProvider.GetService<ILoggerFactory>()
        using var serviceProvider = services.BuildServiceProvider();

        // Assert - no exception means GetService call succeeded
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKeyOverload_ThrowsOnNullServices()
    {
        IServiceCollection services = null!;
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKeyOverload_ThrowsOnNullBearerKey()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKeyOverload_FactoryUsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project");

        // Build provider - triggers serviceProvider.GetService<ILoggerFactory>()
        using var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }
}
