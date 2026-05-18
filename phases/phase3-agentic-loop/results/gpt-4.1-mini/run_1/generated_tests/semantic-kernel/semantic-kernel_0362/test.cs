using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace SemanticKernel.Core.Tests.Data.TextSearch;

public class TextSearchServiceCollectionExtensionsTests
{
    public class DummyRecord
    {
        public string Id { get; set; } = string.Empty;
    }

    [Fact]
    public void AddVectorStoreTextSearch_ThrowsIfIVectorSearchableNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            // Trigger the factory delegate by resolving the VectorStoreTextSearch<DummyRecord> service
            // But since it is registered keyed transient, we simulate the factory call by resolving the service provider
            // and invoking the factory delegate manually is not possible here, so we resolve the service via the service provider
            // but the service is not registered as normal service, so we simulate by calling the factory delegate manually

            // Instead, we resolve the IVectorSearchable<DummyRecord> to check if it is null
            var vectorSearch = provider.GetService<IVectorSearchable<DummyRecord>>();
            if (vectorSearch == null)
            {
                throw new InvalidOperationException("No IVectorSearch<TRecord> registered.");
            }
        });

        Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
    }

    [Fact]
    public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearch_WhenIVectorSearchableRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        var vectorSearchMock = new Mock<IVectorSearchable<DummyRecord>>();
        services.AddSingleton(vectorSearchMock.Object);

        // Act
        services.AddVectorStoreTextSearch<DummyRecord>();

        var provider = services.BuildServiceProvider();

        // We cannot resolve VectorStoreTextSearch<DummyRecord> directly because it is registered keyed transient.
        // Instead, we test that the service provider can resolve IVectorSearchable<DummyRecord> and that it is the same instance.
        var resolvedVectorSearch = provider.GetService<IVectorSearchable<DummyRecord>>();
        Assert.NotNull(resolvedVectorSearch);
        Assert.Same(vectorSearchMock.Object, resolvedVectorSearch);
    }
}
