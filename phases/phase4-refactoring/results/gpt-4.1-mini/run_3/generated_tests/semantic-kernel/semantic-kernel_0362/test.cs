using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.VectorData;
using Xunit;
using Moq;

namespace SemanticKernel.Core.Tests.Data.TextSearch
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        public class PublicDummyRecord { }

        [Fact]
        public void AddVectorStoreTextSearch_ThrowsIfNoVectorSearchableRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add mocks for the other dependencies to avoid nulls but do NOT add IVectorSearchable
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(options);

            // Act
            var provider = services.BuildServiceProvider();

            // Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                provider.GetService<VectorStoreTextSearch<PublicDummyRecord>>());

            Assert.Equal("No IVectorSearch<TRecord> registered.", ex.Message);
        }

        [Fact]
        public void AddVectorStoreTextSearch_RegistersVectorStoreTextSearchSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockVectorSearchable = new Mock<IVectorSearchable<PublicDummyRecord>>();
            var mockStringMapper = new Mock<ITextSearchStringMapper>();
            var mockResultMapper = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            services.AddSingleton(mockVectorSearchable.Object);
            services.AddSingleton(mockStringMapper.Object);
            services.AddSingleton(mockResultMapper.Object);
            services.AddSingleton(options);

            // Act
            services.AddVectorStoreTextSearch<PublicDummyRecord>();

            var provider = services.BuildServiceProvider();

            // Resolve VectorStoreTextSearch<PublicDummyRecord> from the provider
            var vectorStoreTextSearch = provider.GetService<VectorStoreTextSearch<PublicDummyRecord>>();

            // Assert
            Assert.NotNull(vectorStoreTextSearch);
        }
    }
}
