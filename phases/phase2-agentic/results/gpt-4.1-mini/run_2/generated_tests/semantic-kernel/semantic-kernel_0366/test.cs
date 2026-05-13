using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    private class DummyRecord {}

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceId_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockVectorSearchable = new Mock<IVectorSearchable<DummyRecord>>();
        var mockTextEmbeddingGenerationService = new Mock<ITextEmbeddingGenerationService>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        // Setup GetService calls for optional parameters
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper)))
            .Returns(mockStringMapper.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper)))
            .Returns(mockResultMapper.Object);
        mockServiceProvider.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)))
            .Returns(options);

        // Setup GetKeyedService calls for required services
        mockServiceProvider.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>("vectorSearchId"))
            .Returns(mockVectorSearchable.Object);
        mockServiceProvider.Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("embeddingServiceId"))
            .Returns(mockTextEmbeddingGenerationService.Object);

        // We need to intercept the factory delegate passed to AddKeyedTransient to invoke it with our mockServiceProvider
        IServiceCollection interceptedServices = null;
        Func<IServiceProvider, object, VectorStoreTextSearch<DummyRecord>>? factory = null;

        services.AddKeyedTransient = (serviceId, factoryDelegate) =>
        {
            interceptedServices = services;
            factory = factoryDelegate;
            return services;
        };

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>(
            "vectorSearchId",
            "embeddingServiceId",
            stringMapper: null,
            resultMapper: null,
            options: null,
            serviceId: null);

        // Assert
        Assert.NotNull(factory);
        var instance = factory!(mockServiceProvider.Object, null);
        Assert.NotNull(instance);
        Assert.IsType<VectorStoreTextSearch<DummyRecord>>(instance);

        // Verify that GetService was called for optional parameters
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)), Times.Once);

        // Verify that GetKeyedService was called for vector search and embedding generation services
        mockServiceProvider.Verify(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>("vectorSearchId"), Times.Once);
        mockServiceProvider.Verify(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>("embeddingServiceId"), Times.Once);
    }
}
