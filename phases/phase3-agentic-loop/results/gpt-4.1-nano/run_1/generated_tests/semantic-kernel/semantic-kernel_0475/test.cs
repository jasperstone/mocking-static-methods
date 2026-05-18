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
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorIsPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGeneratorFromService_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IEmbeddingGenerator>(new Mock<IEmbeddingGenerator>().Object);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNullAndNoService()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorIsPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGeneratorFromService_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IEmbeddingGenerator>(new Mock<IEmbeddingGenerator>().Object);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenGeneratorIsNullAndNoService()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_CallsGetService_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_CallsGetService_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }
    }
}
