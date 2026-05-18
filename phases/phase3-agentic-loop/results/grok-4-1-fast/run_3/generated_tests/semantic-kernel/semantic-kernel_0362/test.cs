using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test.Extensions;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_WithoutVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<string>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>());
        Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearch_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<string>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_ResolvesNullDependenciesFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());

        // Act
        services.AddVectorStoreTextSearch<string>(stringMapper: null, resultMapper: null, options: null);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithServiceId_RegistersKeyedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());

        const string serviceId = "test-service";

        // Act
        services.AddVectorStoreTextSearch<string>(serviceId: serviceId);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetKeyedService<VectorStoreTextSearch<string>>(serviceId);
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithKeyedVectorSearch_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IVectorSearchable<string>>("test-key", new MockVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<string>("test-key");

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var textSearch = serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>();
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithKeyedVectorSearch_MissingKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());

        // Act
        services.AddVectorStoreTextSearch<string>("nonexistent-key");

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var exception = Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<VectorStoreTextSearch<string>>());
        Assert.Equal("No IVectorSearch<TRecord> for service id nonexistent-key registered.", exception.Message);
    }

    private sealed class MockVectorSearchable : IVectorSearchable<string>
    {
        public object? GetService(Type serviceType, object? serviceKey) => null;

        public IVectorStore<string> VectorStore => throw new NotImplementedException();
        public VectorStoreRecordCollection<string, string> Collection => throw new NotImplementedException();

        public Task<IReadOnlyList<VectorSearchResult<string>>> SearchAsync<TInput>(
            TInput query,
            int count,
            VectorSearchOptions<string>? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class MockStringMapper : ITextSearchStringMapper
    {
        public string MapFromResultToString(object result) => throw new NotImplementedException();
    }

    private sealed class MockResultMapper : ITextSearchResultMapper
    {
        public TextSearchResult MapFromResultToTextSearchResult(object result) => throw new NotImplementedException();
    }
}
