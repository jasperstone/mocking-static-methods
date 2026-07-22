using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using System;

namespace QdrantTests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenServiceProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var embeddingGenerator = new object(); // Replace with a mock or fake if needed
            services.AddTransient<IEmbeddingGenerator>(_ => (IEmbeddingGenerator)embeddingGenerator);
            var provider = services.BuildServiceProvider();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(options);
            Assert.Equal(embeddingGenerator, options?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenServiceDoesNotProvideGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Null(options);
        }
    }
}
