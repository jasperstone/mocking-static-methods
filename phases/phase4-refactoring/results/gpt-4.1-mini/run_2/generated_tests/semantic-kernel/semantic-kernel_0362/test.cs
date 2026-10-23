using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    public class DummyRecord { }

    [Fact]
    public void ResolvingVectorStoreTextSearch_ThrowsIfNoVectorSearchableRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register the VectorStoreTextSearch service with no IVectorSearchable<DummyRecord> registered
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            provider.GetService<VectorStoreTextSearch<DummyRecord>>());

        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void ResolvingVectorStoreTextSearch_SucceedsIfVectorSearchableRegistered()
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

        // Register the VectorStoreTextSearch service
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Act
        var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<DummyRecord>>();

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
    }
}
