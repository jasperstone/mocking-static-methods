using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = sp => null;

            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => QdrantServiceCollectionExtensions.GetStoreOptions(null, optionsProvider));
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderIsNull_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderIsNotNull_ReturnsOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = sp => new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetStoreOptions_EmbeddingGeneratorIsNotNull_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>();
            var serviceProvider = services.BuildServiceProvider();
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = sp => new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EmbeddingGenerator);
        }
    }

    public class MockEmbeddingGenerator : IEmbeddingGenerator, IDisposable
    {
        public void Dispose()
        {
            // Mock implementation
        }

        public object GetService(Type serviceType, object? key)
        {
            // Mock implementation
            return null;
        }

        public void GenerateEmbedding()
        {
            // Mock implementation
        }
    }
}
