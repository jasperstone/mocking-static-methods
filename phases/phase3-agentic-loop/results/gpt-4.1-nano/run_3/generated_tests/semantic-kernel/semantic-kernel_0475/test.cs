using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace VectorData.Tests
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
            var serviceProvider = services.BuildServiceProvider();
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
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
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenOptionsProviderProvidesNullAndServiceHasNoGenerator()
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
            Assert.Null(result.EmbeddingGenerator);
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
            var serviceProvider = services.BuildServiceProvider();
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
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
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenOptionsProviderProvidesNullAndServiceHasNoGenerator()
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
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            mockProvider.Setup(p => p.GetService(typeof(IEmbeddingGenerator))).Returns(mockGenerator.Object);

            var options = new QdrantVectorStoreOptions();
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockProvider.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
            mockProvider.Verify(p => p.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }
    }
}
