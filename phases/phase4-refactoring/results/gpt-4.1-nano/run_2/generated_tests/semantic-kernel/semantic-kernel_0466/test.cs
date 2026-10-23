using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData;

namespace PostgresServiceCollectionExtensionsTests
{
    public class GetStoreOptionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenServiceExists()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
            var options = new PostgresVectorStoreOptions();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(mockEmbeddingGenerator.Object);

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(
                mockServiceProvider.Object,
                sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
            Assert.Same(mockEmbeddingGenerator.Object, result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenServiceDoesNotExist()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var options = new PostgresVectorStoreOptions();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(
                mockServiceProvider.Object,
                sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
            Assert.Null(result?.EmbeddingGenerator);
        }
    }
}
