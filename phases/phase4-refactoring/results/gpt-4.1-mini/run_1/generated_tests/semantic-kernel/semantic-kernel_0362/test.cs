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
    public void AddVectorStoreTextSearch_ThrowsIfNoVectorSearchableRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            provider.GetService<VectorStoreTextSearch<DummyRecord>>());

        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearchSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockVectorSearchable = new Mock<IVectorSearchable<DummyRecord>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        services.AddSingleton(mockVectorSearchable.Object);
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(options);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Resolve VectorStoreTextSearch<DummyRecord> from the provider keyed by default (null)
        var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<DummyRecord>>();

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
    }
}
