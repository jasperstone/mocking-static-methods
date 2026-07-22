using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.Data.Tests
{
    public class TextSearchServiceCollectionExtensionsTests
    {
        [Fact]
        public async Task AddVectorStoreTextSearch_ValidInput_ReturnsServiceCollection()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();

            // Act
            var result = TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<object>(
                services,
                stringMapper,
                resultMapper,
                options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(services, result);
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_NoVectorizedSearch_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<object>(
                    services,
                    stringMapper,
                    resultMapper,
                    options);
            });
        }

        [Fact]
        public async Task AddVectorStoreTextSearch_NoTextEmbeddingGenerationService_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();
            var stringMapper = new Mock<ITextSearchStringMapper>().Object;
            var resultMapper = new Mock<ITextSearchResultMapper>().Object;
            var options = new VectorStoreTextSearchOptions();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                TextSearchServiceCollectionExtensions.AddVectorStoreTextSearch<object>(
                    services,
                    stringMapper,
                    resultMapper,
                    options);
            });
        }
    }
}
