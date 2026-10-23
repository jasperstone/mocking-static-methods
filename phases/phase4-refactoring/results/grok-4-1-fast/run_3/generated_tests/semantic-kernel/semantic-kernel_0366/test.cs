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

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    private class TestRecord { }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_RegistersTransientKeyedService_WhenStringMapperIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new MockVectorSearchable());
        services.AddKeyedSingleton("embedding", new MockTextEmbeddingGenerationService());

        // Act
        var result = services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            stringMapper: null);

        // Assert - Verifies the registration happened (GetService for stringMapper will be called during resolution)
        Assert.Same(services, result);
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(VectorStoreTextSearch<TestRecord>) && 
            Equals(d.ServiceKey, null) &&
            d.Lifetime == ServiceLifetime.Transient);
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_RegistersTransientKeyedService_WhenResultMapperIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new MockVectorSearchable());
        services.AddKeyedSingleton("embedding", new MockTextEmbeddingGenerationService());

        // Act
        var result = services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            resultMapper: null);

        // Assert - Verifies the registration happened (GetService for resultMapper will be called during resolution)
        Assert.Same(services, result);
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(VectorStoreTextSearch<TestRecord>) && 
            Equals(d.ServiceKey, null) &&
            d.Lifetime == ServiceLifetime.Transient);
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_RegistersTransientKeyedService_WhenOptionsIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new MockVectorSearchable());
        services.AddKeyedSingleton("embedding", new MockTextEmbeddingGenerationService());

        // Act
        var result = services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            options: null);

        // Assert - Verifies the registration happened (GetService for options will be called during resolution)
        Assert.Same(services, result);
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(VectorStoreTextSearch<TestRecord>) && 
            Equals(d.ServiceKey, null) &&
            d.Lifetime == ServiceLifetime.Transient);
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_ThrowsInvalidOperationException_WhenVectorSearchServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("embedding", new MockTextEmbeddingGenerationService());

        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "missing-vector",
            textEmbeddingGenerationServiceId: "embedding");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Exception thrown during resolution (triggers the GetKeyedService call on line 123+)
        var exception = Assert.Throws<InvalidOperationException>(() => 
            serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null));
        Assert.Contains("No IVectorizedSearch", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_ThrowsInvalidOperationException_WhenEmbeddingGenerationServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new MockVectorSearchable());

        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "missing-embedding");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - Exception thrown during resolution (triggers the GetKeyedService call on line 123+)
        var exception = Assert.Throws<InvalidOperationException>(() => 
            serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null));
        Assert.Contains("No ITextEmbeddingGenerationService", exception.Message);
    }

    private class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object record) => string.Empty;
    }

    private class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object record) => new TextSearchResult();
    }

    private class MockVectorSearchable : IVectorSearchable<TestRecord>
    {
        public IVectorStoreRecordCollection<TestRecord> VectorizedSearch => 
            new MockVectorStoreRecordCollection<TestRecord>();

        public IAsyncEnumerable<TSearchResult> SearchAsync<TInput>(
            TInput query,
            int maxResults,
            VectorSearchOptions<TestRecord>? options = null,
            CancellationToken cancellationToken = default) where TSearchResult : class
        {
            yield break;
        }
    }

    private class MockVectorStoreRecordCollection<TRecord> : IVectorStoreRecordCollection<TRecord>
    {
        public ValueTask<int> CountAsync(VectorFilter? filter = null, CancellationToken cancellationToken = default) => 
            new(0);

        public IAsyncEnumerable<TRecord> GetBatchAsync(int offset, int count, VectorFilter? filter = null, CancellationToken cancellationToken = default) => 
            AsyncEnumerable.Empty<TRecord>();

        // Minimal implementation - other methods throw NotImplementedException
        public Task UpsertBatchAsync(IEnumerable<TRecord> batch, CancellationToken cancellationToken = default) => 
            Task.CompletedTask;
    }

    private class MockTextEmbeddingGenerationService : ITextEmbeddingGenerationService
    {
        public IAsyncEnumerable<EmbeddingGenerationResult> GenerateEmbeddingsAsync(
            IEmbeddingGenerationInput input,
            CancellationToken cancellationToken = default)
        {
            yield break;
        }
    }
}
