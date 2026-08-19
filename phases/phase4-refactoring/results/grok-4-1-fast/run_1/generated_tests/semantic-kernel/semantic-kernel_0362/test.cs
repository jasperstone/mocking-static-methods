using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_DefaultServiceId_ResolvesDependenciesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable<string>());
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        services.AddVectorStoreTextSearch<string>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_DefaultServiceId_ThrowsWhenVectorSearchMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        services.AddVectorStoreTextSearch<string>();

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>());
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchableServiceId_UsesKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        var vectorSearch = new MockVectorSearchable<string>();
        services.AddKeyedSingleton("vector-search-key", vectorSearch);
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        services.AddVectorStoreTextSearch<string>("vector-search-key");

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchableServiceId_ThrowsWhenKeyedServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        services.AddVectorStoreTextSearch<string>("missing-vector-search");

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() => serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>());
    }

    [Fact]
    public void AddVectorStoreTextSearch_UsesProvidedParameters_WithoutServiceProviderFallback()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable<string>());
        services.AddVectorStoreTextSearch<string>(
            stringMapper: new MockStringMapper(),
            resultMapper: new MockResultMapper(),
            options: new VectorStoreTextSearchOptions());

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();

        // Assert
        Assert.NotNull(textSearch);
    }

    private sealed class MockVectorSearchable<TRecord> : IVectorSearchable<TRecord>
    {
        public VectorStoreRecordCollection<TRecord> Collection => throw new NotImplementedException();
        public IReadOnlyDictionary<string, VectorStoreRecordFieldAttributes> Fields => new Dictionary<string, VectorStoreRecordFieldAttributes>();
        public IVectorStoreRecordFactory<TRecord> RecordFactory => throw new NotImplementedException();

        public IAsyncEnumerable<VectorSearchResult<TRecord>> VectorizedSearchAsync(
            SearchQuery query,
            VectorSearchOptions<TRecord>? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public ValueTask<IAsyncEnumerable<VectorSearchResult<TRecord>>> SearchAsync<TInput>(
            TInput text,
            int maxResults,
            VectorSearchOptions<TRecord>? options = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result) => result?.ToString() ?? string.Empty;
    }

    private sealed class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) 
            => new TextSearchResult(result?.ToString() ?? string.Empty, 1.0);
    }
}
