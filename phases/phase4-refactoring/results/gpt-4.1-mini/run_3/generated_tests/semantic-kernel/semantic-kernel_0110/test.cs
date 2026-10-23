using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.AI;
using Xunit;
using Microsoft.SemanticKernel.Connectors.Google;

public class VertexAIServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVertexAIEmbeddingGenerator_WithBearerKey_RegistersServiceAndResolvesLoggerFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        var modelId = "test-model";
        var bearerKey = "test-bearer-key";
        var location = "us-central1";
        var projectId = "test-project";

        // Add a NullLoggerFactory to the service collection to test GetService call
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);

        // Act
        services.AddVertexAIEmbeddingGenerator(modelId, bearerKey, location, projectId);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var embeddingGenerator = serviceProvider.GetService<IEmbeddingGenerator<string, Embedding<float>>>();
        Assert.NotNull(embeddingGenerator);

        // The logger factory should be resolved by the service provider
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(loggerFactory);
    }
}
