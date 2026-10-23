using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

// Minimal test record
public class TestRecord
{
    public string Content { get; set; } = string.Empty;
}

// Minimal implementations using object for mapper interfaces
public class TestTextSearchStringMapper : ITextSearchStringMapper
{
    public string MapFromResultToString(object result) => ((TestRecord)result).Content;
}

public class TestTextSearchResultMapper : ITextSearchResultMapper
{
    public TextSearchResult MapFromResultToTextSearchResult(object result) => new() { Text = ((TestRecord)result).Content };
}

// Minimal IVectorSearchable implementation - just enough to compile
public class TestVectorSearchable : IVectorSearchable<TestRecord>
{
    public IVectorStoreRecordCollection<TestRecord> VectorizedSearch(ReadOnlyMemory<float> vector, VectorSearchOptions<TestRecord>? options = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public IAsyncEnumerable<VectorSearchResult<TestRecord>> VectorizedSearchAsync(ReadOnlyMemory<float> vector, VectorSearchOptions<TestRecord>? options = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    // Additional required members from IVectorSearchable
    public IAsyncEnumerable<T> SearchAsync<T>(T input, int maxResults, VectorSearchOptions<TestRecord>? searchOptions = null, CancellationToken cancellationToken = default)
        where T : class
        => throw new NotImplementedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_DefaultServiceId_WithVectorSearch_ResolvesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<TestRecord>>(new TestVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>();
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_DefaultServiceId_MissingVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<TestRecord>();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null));
        Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithCustomServiceId_WithVectorSearch_ResolvesSuccessfully()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceId = "test-vector-search";
        services.AddKeyedSingleton<IVectorSearchable<TestRecord>>(serviceId, new TestVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>(serviceId);
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithCustomServiceId_MissingVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceId = "missing-vector-search";

        // Act
        services.AddVectorStoreTextSearch<TestRecord>(serviceId);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null));
        Assert.Equal($"No IVectorSearch<TRecord> for service id {serviceId} registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ResolvesOptionalDependenciesFromContainer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<TestRecord>>(new TestVectorSearchable());
        services.AddSingleton<ITextSearchStringMapper>(new TestTextSearchStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new TestTextSearchResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>();
        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetKeyedService<VectorStoreTextSearch<TestRecord>>(null);

        // Assert - successful resolution without exception
        Assert.True(true);
    }
}
