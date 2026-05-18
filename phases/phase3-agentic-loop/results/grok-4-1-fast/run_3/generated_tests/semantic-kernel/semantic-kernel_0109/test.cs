using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.AI;
using Xunit;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_RegistersKeyedService()
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

        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(null);

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(embeddingGenerator);
        Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerTokenProvider_UsesServiceProviderGetServiceForLoggerFactory()
    {
        // Arrange
        var mockLoggerFactory = new MockLoggerFactory();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(mockLoggerFactory);

        // Act
        services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerTokenProvider: () => new ValueTask<string>("test-token"),
            location: "us-central1",
            projectId: "test-project");

        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(null);

        // Assert - Verify the loggerFactory from serviceProvider.GetService<ILoggerFactory>() was used
        var loggerFactoryField = typeof(VertexAIEmbeddingGenerator).GetField("_loggerFactory", BindingFlags.NonPublic | BindingFlags.Instance);
        var actualLoggerFactory = (ILoggerFactory?)loggerFactoryField?.GetValue(embeddingGenerator);
        Assert.Same(mockLoggerFactory, actualLoggerFactory);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        var result = services.AddVertexAIEmbeddingGenerator(
            modelId: "test-model",
            bearerKey: "test-key",
            location: "us-central1",
            projectId: "test-project");

        var serviceProvider = services.BuildServiceProvider();
        var embeddingGenerator = serviceProvider.GetKeyedService<IEmbeddingGenerator<string, Embedding<float>>>(null);

        // Assert
        Assert.Same(services, result);
        Assert.NotNull(embeddingGenerator);
        Assert.IsType<VertexAIEmbeddingGenerator>(embeddingGenerator);
    }

    [Fact]
    public void AddVertexAIEmbeddingGenerator_NullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - BearerTokenProvider overload
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, () => new ValueTask<string>("token"), "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("token"), null!, "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", () => new ValueTask<string>("token"), "loc", null!));

        // Act & Assert - BearerKey overload  
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator(null!, "key", "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", null!, "loc", "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", null!, "proj"));
        Assert.Throws<ArgumentNullException>(() => services.AddVertexAIEmbeddingGenerator("model", "key", "loc", null!));
    }

    private class MockLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => NullLogger.Instance;
        public void Dispose() { }
    }
}
