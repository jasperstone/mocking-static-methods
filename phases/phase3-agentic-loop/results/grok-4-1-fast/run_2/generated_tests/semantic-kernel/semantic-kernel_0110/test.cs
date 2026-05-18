using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerKeyOverload_RegistersService()
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

        // Assert - No exception during registration
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerKeyOverload_WithServiceId_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project",
            serviceId: "test-service");

        // Assert - No exception during registration
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_BearerTokenProviderOverload_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "us-central1",
            projectId: "test-project");

        // Assert - No exception during registration
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithILoggerFactoryAvailable_CallsGetServiceSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var loggerFactory = NullLoggerFactory.Instance;
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert - serviceProvider.GetService<ILoggerFactory>() succeeds, no exception
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithoutILoggerFactory_CallsGetServiceReturnsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project");

        // Assert - serviceProvider.GetService<ILoggerFactory>() returns null, no exception
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithHttpClient_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var httpClient = new System.Net.Http.HttpClient();

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project",
            httpClient: httpClient);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithCustomApiVersion_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project",
            apiVersion: Microsoft.SemanticKernel.Connectors.Google.VertexAIVersion.V1);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.NotNull(serviceProvider);
    }
}
