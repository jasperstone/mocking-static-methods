using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object };
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(services.BuildServiceProvider(), optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenOptionsProviderProvidesNullGeneratorAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(mockGenerator.Object)
                .BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenServiceHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object };
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(services.BuildServiceProvider(), optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGenerator_WhenOptionsProviderProvidesNullGeneratorAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(mockGenerator.Object)
                .BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenServiceHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => options;

            var serviceProvider = new ServiceCollection().BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }
    }
}
