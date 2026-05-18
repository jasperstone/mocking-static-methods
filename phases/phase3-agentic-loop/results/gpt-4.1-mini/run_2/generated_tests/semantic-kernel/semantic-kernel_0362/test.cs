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
    public void AddVectorStoreTextSearch_ThrowsIfIVectorSearchableNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<DummyRecordPublic>();

        var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            // The factory is called when resolving the keyed transient service
            var _ = provider.GetService<VectorStoreTextSearch<DummyRecordPublic>>();
        });

        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearchWithServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockVectorSearchable = new Mock<IVectorSearchable<DummyRecordPublic>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        services.AddSingleton(mockVectorSearchable.Object);
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(options);

        // Act
        services.AddVectorStoreTextSearch<DummyRecordPublic>();

        var provider = services.BuildServiceProvider();

        // Resolve VectorStoreTextSearch<DummyRecordPublic> from the keyed transient registration
        var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<DummyRecordPublic>>();

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
    }
}
