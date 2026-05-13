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
        public async Task AddVectorStoreTextSearch_WithNullStringMapper_ResultMapperAndOptions_ResolvesFromServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var optionsMock = new Mock<VectorStoreTextSearchOptions>();

            services.AddSingleton<ITextSearchStringMapper>(stringMapperMock.Object);
            services.AddSingleton<ITextSearchResultMapper>(resultMapperMock.Object);
            services.AddSingleton<VectorStoreTextSearchOptions>(optionsMock.Object);

            var serviceProvider = services.BuildServiceProvider();

            // Act
            services.AddVectorStoreTextSearch<MyRecord>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                serviceId: "textSearchServiceId");

            var textSearch = serviceProvider.GetService<ITextSearch>();

            // Assert
            Assert.NotNull(textSearch);
            stringMapperMock.Verify(sm => sm.Map(It.IsAny<MyRecord>()), Times.Once);
            resultMapperMock.Verify(rm => rm.Map(It.IsAny<MyRecord>()), Times.Once);
            optionsMock.Verify(o => o, Times.Once);
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_WithNullVectorSearchable_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddVectorStoreTextSearch<MyRecord>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                serviceId: "textSearchServiceId");

            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ITextSearch>());
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_WithNullTextEmbeddingGenerationService_ThrowsInvalidOperationException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddVectorStoreTextSearch<MyRecord>(
                "vectorSearchServiceId",
                "textEmbeddingGenerationServiceId",
                serviceId: "textSearchServiceId");

            var serviceProvider = services.BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService<ITextSearch>());
        }

        private class MyRecord
        {
        }
    }
}
