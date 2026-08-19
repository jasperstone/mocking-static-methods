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
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullServices_ThrowsArgumentNullException()
    {
        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => ((IServiceCollection)null!).AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", "project"));
        Assert.Equal("services", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullModelId_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => services.AddVertexAIEmbeddingGenerator(null!, tokenProvider, "location", "project"));
        Assert.Equal("modelId", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullTokenProvider_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerTokenProvider", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullLocation_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => services.AddVertexAIEmbeddingGenerator("model", tokenProvider, null!, "project"));
        Assert.Equal("location", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_NullProjectId_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Func<ValueTask<string>> tokenProvider = () => new ValueTask<string>("token");
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => services.AddVertexAIEmbeddingGenerator("model", tokenProvider, "location", null!));
        Assert.Equal("projectId", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullServices_ThrowsArgumentNullException()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => ((IServiceCollection)null!).AddVertexAIEmbeddingGenerator("model", "key", "location", "project"));
        Assert.Equal("services", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_NullBearerKey_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => services.AddVertexAIEmbeddingGenerator("model", null!, "location", "project"));
        Assert.Equal("bearerKey", ex.ParamName);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithTokenProvider_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var httpClient = new HttpClient();

        try
        {
            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                "model", 
                () => new ValueTask<string>("token"), 
                "location", 
                "project",
                httpClient: httpClient);

            // Assert
            Assert.Same(services, result);
            Assert.Single(services);

            var descriptor = services.First();
            Assert.Equal(ServiceDescriptor.ServiceType, descriptor.ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.True(descriptor.IsKeyedService);
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersKeyedSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var httpClient = new HttpClient();

        try
        {
            // Act
            var result = services.AddVertexAIEmbeddingGenerator(
                "model", "key", "location", "project", httpClient: httpClient);

            // Assert
            Assert.Same(services, result);
            Assert.Single(services);

            var descriptor = services.First();
            Assert.Equal(ServiceDescriptor.ServiceType, descriptor.ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.True(descriptor.IsKeyedService);
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithServiceId_UsesProvidedServiceId()
    {
        // Arrange
        var services = new ServiceCollection();
        services.TryAddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        var httpClient = new HttpClient();
        var serviceId = "test-service";

        try
        {
            // Act
            services.AddVertexAIEmbeddingGenerator(
                "model", "key", "location", "project", serviceId, httpClient: httpClient);

            // Assert
            var descriptor = Assert.Single(services);
            Assert.Equal(serviceId, descriptor.ServiceKey);
        }
        finally
        {
            httpClient.Dispose();
        }
    }
}
