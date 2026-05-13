using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.Extensions.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_WithoutVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddVectorStoreTextSearch<string>());

        Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearch_ResolvesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVectorSearch = new Mock<IVectorSearchable<string>>();
        services.AddSingleton(mockVectorSearch.Object);
        services.AddSingleton<ITextSearchStringMapper, MockTextSearchStringMapper>();
        services.AddSingleton<ITextSearchResultMapper, MockTextSearchResultMapper>();
        services.AddSingleton<VectorStoreTextSearchOptions>();

        services.AddVectorStoreTextSearch<string>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithServiceId_ResolvesKeyedServiceSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVectorSearch = new Mock<IVectorSearchable<string>>();
        var serviceId = "test-service";
        services.AddKeyedSingleton<IVectorSearchable<string>>(serviceId, mockVectorSearch.Object);
        services.AddSingleton<ITextSearchStringMapper, MockTextSearchStringMapper>();
        services.AddSingleton<ITextSearchResultMapper, MockTextSearchResultMapper>();
        services.AddSingleton<VectorStoreTextSearchOptions>();

        var textSearchServiceId = "text-search-service";
        services.AddVectorStoreTextSearch<string>(serviceId, serviceId: textSearchServiceId);

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(textSearchServiceId);

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithKeyedVectorSearch_MissingService_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var vectorSearchableServiceId = "missing-vector-search";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddVectorStoreTextSearch<string>(vectorSearchableServiceId));

        Assert.Equal($"No IVectorSearch<TRecord> for service id {vectorSearchableServiceId} registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ResolvesNullParametersFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVectorSearch = new Mock<IVectorSearchable<string>>();
        services.AddSingleton(mockVectorSearch.Object);
        services.AddSingleton<ITextSearchStringMapper, MockTextSearchStringMapper>();
        services.AddSingleton<ITextSearchResultMapper, MockTextSearchResultMapper>();
        services.AddSingleton(new VectorStoreTextSearchOptions());

        services.AddVectorStoreTextSearch<string>(stringMapper: null, resultMapper: null, options: null);

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();

        // Assert
        Assert.NotNull(textSearch);
    }

    private class MockTextSearchStringMapper : ITextSearchStringMapper
    {
        public string MapToString<TRecord>(TRecord record) => record?.ToString() ?? string.Empty;
    }

    private class MockTextSearchResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapToTextSearchResult<TRecord>(TRecord record, float? score = null) =>
            new() { Metadata = record?.ToString() ?? string.Empty, Score = score ?? 0f };
    }
}
