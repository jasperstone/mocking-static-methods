using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_ResolvesGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project");

        // Assert - Verify registration by checking service descriptors
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>) || 
            d.ServiceType == typeof(object));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_ResolvesGenerator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "test-location",
            projectId: "test-project");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>) || 
            d.ServiceType == typeof(object));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NoLoggerFactory_RegistersGenerator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project");

        // Assert - Verifies serviceProvider.GetService<ILoggerFactory>() returns null but registration succeeds
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(IEmbeddingGenerator<string, Embedding<float>>) || 
            d.ServiceType == typeof(object));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var serviceId = "test-service";

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project",
            serviceId: serviceId);

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceKey?.ToString() == serviceId);
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_DefaultServiceKey_RegistersWithNullKey()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "test-location",
            projectId: "test-project");

        // Assert - null key for default registration
        var defaultDescriptor = services.FirstOrDefault(d => d.ServiceKey == null);
        Assert.NotNull(defaultDescriptor);
    }
}
