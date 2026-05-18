using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Register required dependencies as singletons so GetService returns them
        services.AddSingleton<ITextSearchStringMapper>(new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        
        // Register keyed services
        var mockVectorSearch = new Mock<IVectorSearchable<object>>();
        services.AddKeyedSingleton<IVectorSearchable<object>>("vectorSearchId", mockVectorSearch.Object);
        
        var mockEmbeddingGen = new Mock<ITextEmbeddingGenerationService>();
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", mockEmbeddingGen.Object);

        // Act
        services.AddVectorStoreTextSearch<object>(
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            serviceId: "testServiceId");

        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify the factory was invoked and service was created successfully
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<object>>("testServiceId");
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_NullStringMapper_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        
        var mockVectorSearch = new Mock<IVectorSearchable<object>>();
        services.AddKeyedSingleton<IVectorSearchable<object>>("vectorSearchId", mockVectorSearch.Object);
        
        var mockEmbeddingGen = new Mock<ITextEmbeddingGenerationService>();
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", mockEmbeddingGen.Object);

        // Act
        services.AddVectorStoreTextSearch<object>(
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            stringMapper: null, // Explicitly null to trigger GetService
            serviceId: "testServiceId");

        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<object>>("testServiceId");

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_NullResultMapper_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        
        var mockVectorSearch = new Mock<IVectorSearchable<object>>();
        services.AddKeyedSingleton<IVectorSearchable<object>>("vectorSearchId", mockVectorSearch.Object);
        
        var mockEmbeddingGen = new Mock<ITextEmbeddingGenerationService>();
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", mockEmbeddingGen.Object);

        // Act
        services.AddVectorStoreTextSearch<object>(
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            resultMapper: null, // Explicitly null to trigger GetService
            serviceId: "testServiceId");

        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<object>>("testServiceId");

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverloadWithTextEmbeddingGenerationServiceId_NullOptions_UsesGetService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        
        var mockVectorSearch = new Mock<IVectorSearchable<object>>();
        services.AddKeyedSingleton<IVectorSearchable<object>>("vectorSearchId", mockVectorSearch.Object);
        
        var mockEmbeddingGen = new Mock<ITextEmbeddingGenerationService>();
        services.AddKeyedSingleton<ITextEmbeddingGenerationService>("embeddingGenId", mockEmbeddingGen.Object);

        // Act
        services.AddVectorStoreTextSearch<object>(
            vectorSearchServiceId: "vectorSearchId",
            textEmbeddingGenerationServiceId: "embeddingGenId",
            options: null, // Explicitly null to trigger GetService
            serviceId: "testServiceId");

        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<object>>("testServiceId");

        // Assert
        Assert.NotNull(textSearch);
    }

    private class MockTextSearchStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object record) => record?.ToString() ?? string.Empty;
    }

    private class MockTextSearchResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object record) 
            => new TextSearchResult(record?.ToString() ?? string.Empty);
    }
}
