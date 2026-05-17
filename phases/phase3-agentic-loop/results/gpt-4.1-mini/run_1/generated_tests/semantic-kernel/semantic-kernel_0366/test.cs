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
    public class DummyRecord { }

    [Fact]
    public void AddVectorStoreTextSearch_WithServiceId_UsesGetServiceForDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var mockOptions = new VectorStoreTextSearchOptions();

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecord>>();

        // Setup a service provider that returns the mocks when GetService is called
        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchStringMapper))).Returns(mockStringMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(ITextSearchResultMapper))).Returns(mockResultMapper.Object);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(VectorStoreTextSearchOptions))).Returns(mockOptions);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IVectorSearchable<DummyRecord>))).Returns(mockVectorSearch.Object);

        // Add the mocks to the service collection
        services.AddSingleton(serviceProviderMock.Object);
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(mockOptions);
        services.AddSingleton(mockVectorSearch.Object);

        // Add the VectorStoreTextSearch service
        services.AddVectorStoreTextSearch<DummyRecord>();

        var sp = services.BuildServiceProvider();

        // Act
        var vectorStoreTextSearch = sp.GetService<VectorStoreTextSearch<DummyRecord>>();

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithServiceId_ThrowsIfVectorSearchNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var mockOptions = new VectorStoreTextSearchOptions();

        // Add only the mappers and options, but no vector search
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(mockOptions);

        services.AddVectorStoreTextSearch<DummyRecord>();

        var sp = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => sp.GetService<VectorStoreTextSearch<DummyRecord>>());
        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }
}
