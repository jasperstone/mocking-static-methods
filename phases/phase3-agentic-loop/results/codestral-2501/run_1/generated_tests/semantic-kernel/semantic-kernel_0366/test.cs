using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using Xunit;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearch()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var vectorSearchableMock = new Mock<IVectorSearchable<string>>();
        var textEmbeddingGenerationServiceMock = new Mock<ITextEmbeddingGenerationService>();
        var stringMapperMock = new Mock<ITextSearchStringMapper>();
        var resultMapperMock = new Mock<ITextSearchResultMapper>();
        var optionsMock = new Mock<VectorStoreTextSearchOptions>();

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ITextSearchStringMapper)))
            .Returns(stringMapperMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ITextSearchResultMapper)))
            .Returns(resultMapperMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)))
            .Returns(optionsMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetKeyedService<IVectorSearchable<string>>(It.IsAny<string>()))
            .Returns(vectorSearchableMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>(It.IsAny<string>()))
            .Returns(textEmbeddingGenerationServiceMock.Object);

        serviceCollection.AddSingleton(serviceProviderMock.Object);

        // Act
        serviceCollection.AddVectorStoreTextSearch<string>(
            "vectorSearchServiceId",
            "textEmbeddingGenerationServiceId",
            stringMapperMock.Object,
            resultMapperMock.Object,
            optionsMock.Object,
            "serviceId");

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var vectorStoreTextSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>("serviceId");

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetKeyedService<IVectorSearchable<string>>("vectorSearchServiceId"), Times.Once);
        serviceProviderMock.Verify(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("textEmbeddingGenerationServiceId"), Times.Once);
    }
}
