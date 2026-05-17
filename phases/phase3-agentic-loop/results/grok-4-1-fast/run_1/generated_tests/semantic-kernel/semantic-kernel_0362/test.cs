using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;

namespace Microsoft.SemanticKernel.Data.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithoutVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<string>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _ = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(null));
        
        Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithVectorSearch_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVectorSearch = new MockVectorSearchable<string>();
        services.AddSingleton<IVectorSearchable<string>>(mockVectorSearch);

        // Act
        services.AddVectorStoreTextSearch<string>();
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(null);

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ResolvesNullDependenciesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockVectorSearch = new MockVectorSearchable<string>();
        var mockStringMapper = new MockStringMapper();
        var mockResultMapper = new MockResultMapper();
        var mockOptions = new VectorStoreTextSearchOptions();

        services.AddSingleton<IVectorSearchable<string>>(mockVectorSearch);
        services.AddSingleton<ITextSearchStringMapper>(mockStringMapper);
        services.AddSingleton<ITextSearchResultMapper>(mockResultMapper);
        services.AddSingleton<VectorStoreTextSearchOptions>(mockOptions);

        // Act
        services.AddVectorStoreTextSearch<string>();
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(null);

        // Assert
        Assert.NotNull(textSearch);
    }

    private class MockVectorStoreCollection<TKey, TRecord> : IVectorStoreCollection<TKey, TRecord>
    {
        public IReadOnlyList<VectorStoreRecordId> Ids => new List<VectorStoreRecordId>();
        public IReadOnlyList<VectorStoreRecordMetadata> Metadata => new List<VectorStoreRecordMetadata>();
        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default) => ValueTask.FromResult(0);
        public IAsyncEnumerable<TRecord> GetBatchAsync(IEnumerable<VectorStoreRecordId> ids, CancellationToken cancellationToken = default) => Enumerable.Empty<TRecord>().ToAsyncEnumerable();
        public ValueTask<TRecord?> GetAsync(VectorStoreRecordId id, CancellationToken cancellationToken = default) => ValueTask.FromResult<TRecord?>(default);
        public IAsyncEnumerable<TRecord> GetAsync(VectorStoreRecordRetrievalOptions? options = null, CancellationToken cancellationToken = default) => Enumerable.Empty<TRecord>().ToAsyncEnumerable();
        public async IAsyncEnumerable<TRecord> UpsertBatchAsync(IEnumerable<TRecord> records, VectorStoreRecordUpdateOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield break;
        }
        public ValueTask<VectorStoreRecordId> UpsertAsync(TRecord record, VectorStoreRecordUpdateOptions? options = null, CancellationToken cancellationToken = default) => ValueTask.FromResult(new VectorStoreRecordId(""));
        public ValueTask DeleteAsync(VectorStoreRecordId id, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
        public ValueTask DeleteBatchAsync(IEnumerable<VectorStoreRecordId> ids, CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
    }

    private class MockVectorSearchable<TRecord> : IVectorSearchable<TRecord>
    {
        public IVectorStoreCollection<TKey, TRecord> VectorStoreCollection => new MockVectorStoreCollection<string, TRecord>();

        public IAsyncEnumerable<VectorSearchResult<TRecord>> VectorizedSearchAsync(
            ReadOnlyMemory<float> queryVector, 
            VectorSearchOptions<TRecord>? options = null, 
            CancellationToken cancellationToken = default) => 
            Enumerable.Empty<VectorSearchResult<TRecord>>().ToAsyncEnumerable();

        public IAsyncEnumerable<VectorSearchResult<TRecord>> VectorizedSearchAsync(
            Embedding<float> queryVector, 
            VectorSearchOptions<TRecord>? options = null, 
            CancellationToken cancellationToken = default) => 
            Enumerable.Empty<VectorSearchResult<TRecord>>().ToAsyncEnumerable();

        public IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(
            TInput query, 
            int maxResults, 
            VectorSearchOptions<TRecord>? options = null, 
            CancellationToken cancellationToken = default) => 
            Enumerable.Empty<VectorSearchResult<TRecord>>().ToAsyncEnumerable();
    }

    private class MockStringMapper : ITextSearchStringMapper
    {
        public string? MapFromResultToString(object result) => result?.ToString();
    }

    private class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) => 
            new TextSearchResult(result?.ToString() ?? "", 1.0);
    }
}
