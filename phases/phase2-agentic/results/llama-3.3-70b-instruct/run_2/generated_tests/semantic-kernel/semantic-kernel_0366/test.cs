using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddVectorStoreTextSearch_WithNullStringMapper_ResultMapperAndOptions_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchableServiceId = "vectorSearchableServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var serviceId = "serviceId";

            // Act
            services.AddVectorStoreTextSearch<MyRecord>(
                vectorSearchableServiceId,
                textEmbeddingGenerationServiceId,
                serviceId: serviceId);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_WithNullStringMapper_ResultMapperAndOptions_SearchAsync_ReturnsResults()
        {
            // Arrange
            var services = new ServiceCollection();
            var vectorSearchableServiceId = "vectorSearchableServiceId";
            var textEmbeddingGenerationServiceId = "textEmbeddingGenerationServiceId";
            var serviceId = "serviceId";

            var vectorSearchableMock = new Mock<IVectorSearchable<MyRecord>>();
            var textEmbeddingGenerationMock = new Mock<ITextEmbeddingGenerationService>();

            services.AddKeyedTransient<IVectorSearchable<MyRecord>>(vectorSearchableServiceId, _ => vectorSearchableMock.Object);
            services.AddKeyedTransient<ITextEmbeddingGenerationService>(textEmbeddingGenerationServiceId, _ => textEmbeddingGenerationMock.Object);

            services.AddVectorStoreTextSearch<MyRecord>(
                vectorSearchableServiceId,
                textEmbeddingGenerationServiceId,
                serviceId: serviceId);

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            var results = await textSearch.SearchAsync("query");

            // Assert
            Assert.NotNull(results);
        }

        private class MyRecord
        {
        }
    }
}
