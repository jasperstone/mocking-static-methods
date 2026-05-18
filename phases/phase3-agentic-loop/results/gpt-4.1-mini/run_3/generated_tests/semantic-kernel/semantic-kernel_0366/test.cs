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
    public class DummyRecordPublic { }

    [Fact]
    public void AddVectorStoreTextSearch_WithServiceProviderGetServiceCalled()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var mockOptions = new VectorStoreTextSearchOptions();

        var mockVectorSearch = new Mock<IVectorSearchable<DummyRecordPublic>>();

        // We need to register the mocks in the service collection so that the factory can resolve them
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(mockOptions);
        services.AddSingleton(mockVectorSearch.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecordPublic>();

        // Build the provider and resolve the VectorStoreTextSearch<DummyRecordPublic> to trigger the factory
        var provider = services.BuildServiceProvider();

        var textSearch = provider.GetService<VectorStoreTextSearch<DummyRecordPublic>>();

        // Assert
        Assert.NotNull(textSearch);
    }
}
