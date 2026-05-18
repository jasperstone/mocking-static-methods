using System;
using System.Collections.Generic;
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
    public void AddVectorStoreTextSearch_ObsoleteOverload_CallsGetServiceOnServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        var getServiceCalls = new List<Type>();
        services.AddSingleton<IServiceProvider>(new CapturingServiceProvider(getServiceCalls));
        services.AddSingleton<ITextSearchStringMapper>(new object());
        services.AddSingleton<ITextSearchResultMapper>(new object());
        services.AddSingleton<VectorStoreTextSearchOptions>(new VectorStoreTextSearchOptions());

        // Act
        TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
            services,
            "vectorSearchId",
            "embeddingGenId");

        // Assert - Verify GetService was called for the expected types (line ~123 and nearby)
        Assert.Contains(typeof(ITextSearchStringMapper), getServiceCalls);
        Assert.Contains(typeof(ITextSearchResultMapper), getServiceCalls);
        Assert.Contains(typeof(VectorStoreTextSearchOptions), getServiceCalls);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenVectorSearchNotFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var getServiceCalls = new List<Type>();
        services.AddSingleton<IServiceProvider>(new CapturingServiceProvider(getServiceCalls));

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
                services,
                "vectorSearchId",
                "embeddingGenId"));

        Assert.Equal("No IVectorizedSearch<TestRecord> for service id vectorSearchId registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenEmbeddingGenerationNotFound()
    {
        // Arrange
        var services = new ServiceCollection();
        var getServiceCalls = new List<Type>();
        services.AddSingleton<IServiceProvider>(new CapturingServiceProvider(getServiceCalls));
        services.AddKeyedSingleton<IVectorSearchable<TestRecord>>("vectorSearchId", new object());

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<TestRecord>(
                services,
                "vectorSearchId",
                "embeddingGenId"));

        Assert.StartsWith("No ITextEmbeddingGenerationService for service id embeddingGenId registered.", exception.Message);
    }

    private class CapturingServiceProvider : IServiceProvider
    {
        private readonly List<Type> _calls = new();
        public List<Type> Calls => _calls;

        public object? GetService(Type serviceType)
        {
            _calls.Add(serviceType);
            return null;
        }
    }
}
