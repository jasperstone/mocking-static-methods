using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithEndpoint_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        // Use NullLoggerFactory to satisfy ILoggerFactory dependency
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

        var endpoint = new Uri("https://fake-endpoint");
        var apiKey = "fake-api-key";
        var serviceId = "test-service";

        // Act
        var returnedServices = services.AddHuggingFaceTextEmbeddingGeneration(
            endpoint,
            apiKey,
            serviceId);

        // Assert
        Assert.Same(services, returnedServices);
    }

    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_WithEndpoint_ServiceCanBeResolved()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory>(NullLoggerFactory.Instance);

        var endpoint = new Uri("https://fake-endpoint");
        var apiKey = "fake-api-key";
        var serviceId = "test-service";

        services.AddHuggingFaceTextEmbeddingGeneration(endpoint, apiKey, serviceId);

        // Act
        var serviceProvider = services.BuildServiceProvider();

        // The service is registered as keyed singleton, so resolve by service type only
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        Assert.NotNull(embeddingService);
        Assert.IsType<HuggingFaceTextEmbeddingGenerationService>(embeddingService);
    }
}
