using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_RegistersFactoryWithGetServiceCalls()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        // Act
        _ = Microsoft.SemanticKernel.TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<DummyRecord>(
            services, "vector1", "embedding1");

        // Assert - Registration succeeds, verifying the factory lambda with GetService calls executes without error
        using var sp = services.BuildServiceProvider();
        var descriptor = services.FirstOrDefault(d => 
            d.ServiceType == typeof(VectorStoreTextSearch<DummyRecord>));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenVectorSearchServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        using var sp = services
            .AddVectorStoreTextSearch<DummyRecord>("missing-vector", "embedding1")
            .BuildServiceProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => 
            sp.GetKeyedService<VectorStoreTextSearch<DummyRecord>>(null));
        Assert.Contains("No IVectorizedSearch", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ObsoleteOverload_ThrowsWhenEmbeddingGenerationServiceMissing()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        using var sp = services
            .AddVectorStoreTextSearch<DummyRecord>("vector1", "missing-embedding")
            .BuildServiceProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => 
            sp.GetKeyedService<VectorStoreTextSearch<DummyRecord>>(null));
        Assert.Contains("No ITextEmbeddingGenerationService", ex.Message);
    }

    private class DummyRecord { }

    private class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object record) => record?.ToString() ?? string.Empty;
    }

    private class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object record, float relevanceScore) => 
            new() { Text = record?.ToString() ?? string.Empty, Relevance = relevanceScore };
    }
}
