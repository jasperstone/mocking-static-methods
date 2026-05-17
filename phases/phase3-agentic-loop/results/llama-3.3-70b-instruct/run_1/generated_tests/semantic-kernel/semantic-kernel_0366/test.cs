using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Data;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SemanticKernel.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddVectorStoreTextSearch_WithServiceId_ServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Act
            services.AddVectorStoreTextSearch<object>(
                stringMapper: stringMapperMock.Object,
                resultMapper: resultMapperMock.Object,
                options: options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_WithServiceIdAndVectorSearchableServiceId_ServiceIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapperMock = new Mock<ITextSearchStringMapper>();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Act
            services.AddVectorStoreTextSearch<object>(
                "vectorSearchableServiceId",
                stringMapper: stringMapperMock.Object,
                resultMapper: resultMapperMock.Object,
                options: options);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var textSearch = serviceProvider.GetService<ITextSearch>();
            Assert.NotNull(textSearch);
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_WithServiceIdAndNullStringMapper_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var resultMapperMock = new Mock<ITextSearchResultMapper>();
            var options = new VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddVectorStoreTextSearch<object>(
                stringMapper: null,
                resultMapper: resultMapperMock.Object,
                options: options));
        }
    }
}
