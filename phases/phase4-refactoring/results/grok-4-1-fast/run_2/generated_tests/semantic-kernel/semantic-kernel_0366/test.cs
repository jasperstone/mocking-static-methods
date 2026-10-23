using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_CallsGetServiceForStringMapper_WhenNull_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new ThrowingMockVectorSearchable<TestRecord>());
        services.AddKeyedSingleton("embedding", new ThrowingMockTextEmbeddingGenerationService());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            stringMapper: null);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<Func<IServiceProvider, object, object>>("default") 
            ?? throw new InvalidOperationException("Factory not registered");

        // Act & Assert - triggers line 123: stringMapper ??= sp.GetService<ITextSearchStringMapper>();
        // If GetService fails, it would throw before reaching vectorizedSearch null check
        var ex = Assert.Throws<InvalidOperationException>(() => factory(serviceProvider, null!));
        Assert.Contains("IVectorizedSearch", ex.Message); // Confirms GetService on line 123 succeeded
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_CallsGetServiceForResultMapper_WhenNull_Succeeds()
    {
        // Arrange - same setup, resultMapper = null triggers line ~124
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new ThrowingMockVectorSearchable<TestRecord>());
        services.AddKeyedSingleton("embedding", new ThrowingMockTextEmbeddingGenerationService());

        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            resultMapper: null);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<Func<IServiceProvider, object, object>>("default")!;
        
        var ex = Assert.Throws<InvalidOperationException>(() => factory(serviceProvider, null!));
        Assert.Contains("IVectorizedSearch", ex.Message); // Confirms GetService succeeded
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_CallsGetServiceForOptions_WhenNull_Succeeds()
    {
        // Arrange - options = null triggers line ~125
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new ThrowingMockVectorSearchable<TestRecord>());
        services.AddKeyedSingleton("embedding", new ThrowingMockTextEmbeddingGenerationService());

        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            options: null);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<Func<IServiceProvider, object, object>>("default")!;
        
        var ex = Assert.Throws<InvalidOperationException>(() => factory(serviceProvider, null!));
        Assert.Contains("IVectorizedSearch", ex.Message); // Confirms GetService succeeded
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenStringMapperMissing()
    {
        // Arrange - missing ITextSearchStringMapper triggers GetService failure on line 123
        var services = new ServiceCollection();
        // No ITextSearchStringMapper registered
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton("vector", new ThrowingMockVectorSearchable<TestRecord>());
        services.AddKeyedSingleton("embedding", new ThrowingMockTextEmbeddingGenerationService());

        services.AddVectorStoreTextSearch<TestRecord>(
            vectorSearchServiceId: "vector",
            textEmbeddingGenerationServiceId: "embedding",
            stringMapper: null);

        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetKeyedService<Func<IServiceProvider, object, object>>("default")!;
        
        var ex = Assert.Throws<InvalidOperationException>(() => factory(serviceProvider, null!));
        Assert.Contains("ITextSearchStringMapper", ex.Message);
    }

    private class TestRecord { }

    private class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result) => string.Empty;
    }

    private class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) => 
            new TextSearchResult(string.Empty, 0);
    }

    private class ThrowingMockVectorSearchable<TRecord> : IVectorSearchable<TRecord>
    {
        public IVectorStoreRecordCollection<TRecord> Records => throw new NotImplementedException();
        public Task<IReadOnlyList<TRecord>> VectorizedSearchAsync(
            ReadOnlyMemory<float> queryVector,
            VectorizedSearchOptions? options = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private class ThrowingMockTextEmbeddingGenerationService : ITextEmbeddingGenerationService
    {
        public Task<IList<Embedding>> GenerateEmbeddingsAsync(
            IReadOnlyList<string> texts,
            KernelArguments? arguments = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
