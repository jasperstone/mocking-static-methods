using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    public class TestRecord { }

    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithoutVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var serviceProvider = services.AddVectorStoreTextSearch<TestRecord>().BuildServiceProvider();
        var factory = serviceProvider.GetRequiredKeyedService<IKeyedServiceFactory<VectorStoreTextSearch<TestRecord>, object?>>("");
        var ex = Assert.Throws<InvalidOperationException>(() => factory.CreateService(serviceProvider));
        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithVectorSearch_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<TestRecord>>(new MockVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredKeyedService<IKeyedServiceFactory<VectorStoreTextSearch<TestRecord>, object?>>("");
        var textSearch = factory.CreateService(serviceProvider);
        Assert.IsType<VectorStoreTextSearch<TestRecord>>(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_SecondOverload_WithoutKeyedVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var serviceProvider = services.AddVectorStoreTextSearch<TestRecord>("test").BuildServiceProvider();
        var factory = serviceProvider.GetRequiredKeyedService<IKeyedServiceFactory<VectorStoreTextSearch<TestRecord>, object?>>("");
        var ex = Assert.Throws<InvalidOperationException>(() => factory.CreateService(serviceProvider));
        Assert.Equal("No IVectorSearch<TRecord> for service id test registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_SecondOverload_WithKeyedVectorSearch_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSingleton("test", new MockVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>("test");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredKeyedService<IKeyedServiceFactory<VectorStoreTextSearch<TestRecord>, object?>>("");
        var textSearch = factory.CreateService(serviceProvider);
        Assert.IsType<VectorStoreTextSearch<TestRecord>>(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ResolvesNullDependenciesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<TestRecord>>(new MockVectorSearchable());
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        // Act
        services.AddVectorStoreTextSearch<TestRecord>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredKeyedService<IKeyedServiceFactory<VectorStoreTextSearch<TestRecord>, object?>>("");
        var textSearch = factory.CreateService(serviceProvider);
        Assert.IsType<VectorStoreTextSearch<TestRecord>>(textSearch);
    }

    private sealed class MockVectorSearchable : IVectorSearchable<TestRecord>
    {
        public IVectorStoreRecordCollection<string, TestRecord> Records => throw new NotImplementedException();
        public IReadOnlyDictionary<string, IVectorStoreRecordCollection<string, TestRecord>> RecordCollections => new Dictionary<string, IVectorStoreRecordCollection<string, TestRecord>>();
        public VectorStoreRecordMetadataCollection Metadata => new();

        public IAsyncEnumerable<VectorSearchResult<TestRecord>> SearchAsync<TInput>(
            TInput input,
            int count,
            VectorSearchOptions<TestRecord>? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result) => "";
    }

    private sealed class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) => new("", 0);
    }
}
