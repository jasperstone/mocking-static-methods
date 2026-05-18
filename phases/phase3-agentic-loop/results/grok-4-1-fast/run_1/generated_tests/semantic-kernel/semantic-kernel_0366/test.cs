using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register required dependencies as singletons so GetService returns them
        services.AddSingleton<ITextSearchStringMapper>(provider => new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(provider => new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton<IVectorSearchable<TestRecord>>("vectorSearchId", Mock.Of<IVectorSearchable<TestRecord>>());
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", Mock.Of<ITextEmbeddingGenerationService>());

        // Act
        var result = TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
            services,
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId");

        // Assert - Registration succeeded (no exception thrown during registration)
        Assert.NotNull(result);
        Assert.NotEmpty(services);

        // Verify the factory can resolve without exceptions - uses default serviceId = null
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_MissingStringMapper_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(provider => new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(provider => new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton<IVectorSearchable<TestRecord>>("vectorSearchId", Mock.Of<IVectorSearchable<TestRecord>>());
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", Mock.Of<ITextEmbeddingGenerationService>());

        // Act & Assert - null stringMapper triggers sp.GetService<ITextSearchStringMapper>()
        var result = TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
            services,
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            stringMapper: null);

        Assert.NotNull(result);

        // Verify resolution works
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_MissingOptions_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(provider => new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(provider => new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton<IVectorSearchable<TestRecord>>("vectorSearchId", Mock.Of<IVectorSearchable<TestRecord>>());
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", Mock.Of<ITextEmbeddingGenerationService>());

        // Act & Assert - null options triggers sp.GetService<VectorStoreTextSearchOptions>()
        var result = TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
            services,
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            options: null);

        Assert.NotNull(result);

        // Verify resolution works
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenVectorSearchMissing()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(provider => new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(provider => new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", Mock.Of<ITextEmbeddingGenerationService>());

        var resultServices = TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
            services,
            vectorSearchServiceId: "missingVectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert - triggers the sp.GetKeyedService<IVectorSearchable<TRecord>>(vectorSearchServiceId) null check
        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null));
        Assert.Contains("missingVectorSearchId", exception.Message);
    }

    private class TestRecord
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private class MockTextSearchStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result) => ((TestRecord)result).Content;
    }

    private class MockTextSearchResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) => new TextSearchResult("test");
    }
}
