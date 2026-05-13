using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace SemanticKernel.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        private class DummyRecord { }

        [Fact]
        public void AddVectorStoreTextSearch_ThrowsIfIVectorSearchableNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                services.AddVectorStoreTextSearch<DummyRecord>());

            Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearchWithProvidedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<DummyRecord>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(options);

            // Act
            services.AddVectorStoreTextSearch<DummyRecord>();

            // Build provider and resolve the VectorStoreTextSearch<DummyRecord>
            var provider = services.BuildServiceProvider();

            // The registration is keyed transient, so we need to get the service via the keyed service extension
            var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<DummyRecord>>();

            // Assert
            Assert.NotNull(vectorStoreTextSearch);
        }
    }
}
