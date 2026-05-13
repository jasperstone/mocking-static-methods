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
    private class DummyRecord
    {
        public string Text { get; set; } = string.Empty;
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithoutServiceId_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecord>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(mockVectorSearch.Object);

        services.AddSingleton(serviceProviderMock.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Assert
        var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<DummyRecord>>();
        Assert.Null(vectorStoreTextSearch); // Because AddKeyedTransient registers keyed, not default

        // Instead, resolve via the keyed service factory
        var keyedFactory = provider.GetService<Func<string?, VectorStoreTextSearch<DummyRecord>>>();
        Assert.NotNull(keyedFactory);

        var instance = keyedFactory!(null);
        Assert.NotNull(instance);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchableServiceId_RegistersService()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecord>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var serviceId = "myVectorSearch";

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);

        var keyedServiceProviderMock = new Mock<IServiceProvider>();
        keyedServiceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(serviceId)).Returns(mockVectorSearch.Object);

        // We simulate the service provider passed to the factory as a composite that supports GetService and GetKeyedService
        var compositeServiceProviderMock = new Mock<IServiceProvider>();
        compositeServiceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        compositeServiceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        compositeServiceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);
        compositeServiceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(serviceId)).Returns(mockVectorSearch.Object);

        services.AddSingleton(compositeServiceProviderMock.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>(serviceId);

        var provider = services.BuildServiceProvider();

        var keyedFactory = provider.GetService<Func<string?, VectorStoreTextSearch<DummyRecord>>>();
        Assert.NotNull(keyedFactory);

        var instance = keyedFactory!(null);
        Assert.NotNull(instance);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceIdAndTextEmbeddingGenerationServiceId_CallsGetService()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchServiceId = "vectorSearchId";
        var textEmbeddingGenerationServiceId = "embeddingGenId";

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecord>>();
        var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);

        // Setup GetKeyedService calls
        serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(vectorSearchServiceId)).Returns(mockVectorSearch.Object);
        serviceProviderMock.Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId)).Returns(mockGenerationService.Object);

        services.AddSingleton(serviceProviderMock.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>(vectorSearchServiceId, textEmbeddingGenerationServiceId);

        var provider = services.BuildServiceProvider();

        var keyedFactory = provider.GetService<Func<string?, VectorStoreTextSearch<DummyRecord>>>();
        Assert.NotNull(keyedFactory);

        var instance = keyedFactory!(null);
        Assert.NotNull(instance);

        // Verify that GetService was called for stringMapper, resultMapper, options
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchStringMapper)), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(ITextSearchResultMapper)), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetService(typeof(VectorStoreTextSearchOptions)), Times.AtLeastOnce);

        // Verify that GetKeyedService was called for vectorSearchServiceId and textEmbeddingGenerationServiceId
        serviceProviderMock.Verify(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(vectorSearchServiceId), Times.AtLeastOnce);
        serviceProviderMock.Verify(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId), Times.AtLeastOnce);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceIdAndTextEmbeddingGenerationServiceId_ThrowsIfVectorSearchNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchServiceId = "vectorSearchId";
        var textEmbeddingGenerationServiceId = "embeddingGenId";

        var mockGenerationService = new Mock<ITextEmbeddingGenerationService>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);

        // Vector search is null to simulate missing registration
        serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(vectorSearchServiceId)).Returns((IVectorSearchable<DummyRecord>?)null);
        serviceProviderMock.Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId)).Returns(mockGenerationService.Object);

        services.AddSingleton(serviceProviderMock.Object);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddVectorStoreTextSearch<DummyRecord>(vectorSearchServiceId, textEmbeddingGenerationServiceId));

        Assert.Contains($"No IVectorizedSearch<TRecord> for service id {vectorSearchServiceId} registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceIdAndTextEmbeddingGenerationServiceId_ThrowsIfGenerationServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchServiceId = "vectorSearchId";
        var textEmbeddingGenerationServiceId = "embeddingGenId";

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecord>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(options);

        serviceProviderMock.Setup(sp => sp.GetKeyedService<IVectorSearchable<DummyRecord>>(vectorSearchServiceId)).Returns(mockVectorSearch.Object);
        // Generation service is null to simulate missing registration
        serviceProviderMock.Setup(sp => sp.GetKeyedService<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId)).Returns((ITextEmbeddingGenerationService?)null);

        services.AddSingleton(serviceProviderMock.Object);

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddVectorStoreTextSearch<DummyRecord>(vectorSearchServiceId, textEmbeddingGenerationServiceId));

        Assert.Contains($"No ITextEmbeddingGenerationService for service id {textEmbeddingGenerationServiceId} registered.", ex.Message);
    }
}
