using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Xunit;

namespace Microsoft.SemanticKernel.Test;

public class TextSearchServiceCollectionExtensionsTests
{
    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithoutVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddVectorStoreTextSearch<string>();
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetServices<ServiceDescriptor>()
            .Single(s => s.ServiceType == typeof(VectorStoreTextSearch<string>))
            .ImplementationFactory as Func<IServiceProvider, object>;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory!(serviceProvider, null));
        Assert.Equal("No IVectorSearch<TRecord> registered.", exception.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_WithVectorSearch_Succeeds()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());
        services.AddVectorStoreTextSearch<string>();
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetServices<ServiceDescriptor>()
            .Single(s => s.ServiceType == typeof(VectorStoreTextSearch<string>))
            .ImplementationFactory as Func<IServiceProvider, object>;

        // Act
        var textSearch = factory!(serviceProvider, null) as VectorStoreTextSearch<string>;

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_FirstOverload_ResolvesNullParametersFromServiceProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IVectorSearchable<string>>(new MockVectorSearchable());
        services.AddSingleton<ITextSearchStringMapper>(new MockStringMapper());
        services.AddSingleton<ITextSearchResultMapper>(new MockResultMapper());
        services.AddSingleton(new VectorStoreTextSearchOptions());
        services.AddVectorStoreTextSearch<string>(stringMapper: null, resultMapper: null, options: null);
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetServices<ServiceDescriptor>()
            .Single(s => s.ServiceType == typeof(VectorStoreTextSearch<string>))
            .ImplementationFactory as Func<IServiceProvider, object>;

        // Act
        var textSearch = factory!(serviceProvider, null) as VectorStoreTextSearch<string>;

        // Assert
        Assert.NotNull(textSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_SecondOverload_WithoutKeyedVectorSearch_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddVectorStoreTextSearch<string>("missing-service-id");
        var serviceProvider = services.BuildServiceProvider();
        
        var factory = serviceProvider.GetServices<ServiceDescriptor>()
            .Single(s => s.ServiceType == typeof(VectorStoreTextSearch<string>))
            .ImplementationFactory as Func<IServiceProvider, object>;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => factory!(serviceProvider, null));
        Assert.Contains("missing-service-id", exception.Message);
    }

    private sealed class MockVectorSearchable : IVectorSearchable<string>
    {
        public IAsyncEnumerable<VectorSearchResult<string>> SearchAsync<TInput>(
            TInput input,
            int count,
            VectorSearchOptions<string>? options = null,
            CancellationToken cancellationToken = default) => 
            throw new NotImplementedException();
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
