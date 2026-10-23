using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddQdrantVectorStore_WithOptionsProviderAndGenerator_ReturnsExpected()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);

            // Act
            services.AddQdrantVectorStore(
                host: "localhost",
                port: 1234,
                https: false,
                apiKey: "key",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Assert
            var store = provider.GetService<QdrantVectorStore>();
            Assert.NotNull(store);
            Assert.IsType<QdrantVectorStore>(store);
        }
    }
}
