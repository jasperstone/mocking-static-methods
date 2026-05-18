using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

public class OllamaServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOllamaTextEmbeddingGeneration_ShouldAddServiceToCollection()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        var ollamaClientMock = new Mock<OllamaApiClient>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(OllamaApiClient)))
            .Returns(ollamaClientMock.Object);

        serviceCollection.AddSingleton(serviceProviderMock.Object);

        // Act
        serviceCollection.AddOllamaTextEmbeddingGeneration();

        // Assert
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var embeddingService = serviceProvider.GetService<ITextEmbeddingGenerationService>();

        Assert.NotNull(embeddingService);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(OllamaApiClient)), Times.Once);
    }
}
