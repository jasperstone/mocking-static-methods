using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.HuggingFace;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Connectors.HuggingFace;

public class HuggingFaceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHuggingFaceTextEmbeddingGeneration_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockLoggerFactory = new Mock<ILoggerFactory>();

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        // We need to register the service provider so that the factory can resolve ILoggerFactory
        services.AddSingleton(sp => mockServiceProvider.Object);

        var endpoint = new Uri("https://fake-endpoint");

        // Act
        var result = services.AddHuggingFaceTextEmbeddingGeneration(
            endpoint,
            apiKey: "fake-api-key",
            serviceId: "test-service",
            httpClient: null);

        // Build the service provider to invoke the factory delegate
        var builtProvider = services.BuildServiceProvider();

        // Resolve the keyed service to trigger the factory delegate and thus the GetService call
        var embeddingService = builtProvider.GetService<ITextEmbeddingGenerationService>();

        // Assert
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ILoggerFactory)), Times.AtLeastOnce());
        Assert.NotNull(embeddingService);
        Assert.Same(result, services);
    }
}
