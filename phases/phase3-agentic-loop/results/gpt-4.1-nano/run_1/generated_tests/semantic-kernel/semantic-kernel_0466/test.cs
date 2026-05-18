using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData.PgVector;

namespace Microsoft.Extensions.VectorData.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenServiceAvailable()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(mockEmbeddingGenerator);

            var options = new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(
                mockServiceProvider.Object,
                sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockEmbeddingGenerator, result?.EmbeddingGenerator);
        }
    }
}
