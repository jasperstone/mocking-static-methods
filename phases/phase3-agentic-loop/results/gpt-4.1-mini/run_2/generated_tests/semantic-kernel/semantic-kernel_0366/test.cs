using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    public class TestRecord {}

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceId_RegistersServiceAndResolves()
    {
        // Arrange
        var services = new ServiceCollection();

        var mockVectorSearchable = new Mock<IVectorSearchable<TestRecord>>();
        var mockStringMapper = new Mock<ITextSearchStringMapper>();
        var mockResultMapper = new Mock<ITextSearchResultMapper>();
        var options = new VectorStoreTextSearchOptions();

        // Register dependencies in service provider
        services.AddSingleton(mockStringMapper.Object);
        services.AddSingleton(mockResultMapper.Object);
        services.AddSingleton(options);

        // Register the keyed service manually to simulate GetKeyedService
        services.AddSingleton(mockVectorSearchable.Object);

        // Act
        services.AddVectorStoreTextSearch<TestRecord>(
            "myVectorSearchServiceId",
            stringMapper: null,
            resultMapper: null,
            options: null,
            serviceId: "myServiceId");

        var provider = services.BuildServiceProvider();

        // Resolve the VectorStoreTextSearch<TestRecord> using the keyed service
        var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<TestRecord>>();

        // Assert
        Assert.NotNull(vectorStoreTextSearch);
    }

    [Fact]
    public void AddVectorStoreTextSearch_WithVectorSearchServiceId_ThrowsIfVectorSearchableNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<TestRecord>(
            "missingServiceId",
            stringMapper: null,
            resultMapper: null,
            options: null,
            serviceId: "myServiceId");

        var provider = services.BuildServiceProvider();

        // Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            // This triggers the factory delegate that calls GetKeyedService and throws
            var _ = provider.GetService<VectorStoreTextSearch<TestRecord>>();
        });
        Assert.Contains("No IVectorizedSearch<TRecord> for service id missingServiceId registered.", ex.Message);
    }
}
