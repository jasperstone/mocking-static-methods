using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Qdrant.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithoutChange_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithGenerator_WhenGeneratorExistsInServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
            Assert.NotSame(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithoutChange_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = null };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithGenerator_WhenGeneratorExistsInServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var options = new QdrantCollectionOptions { EmbeddingGenerator = null };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
            Assert.NotSame(options, result);
        }

        [Fact]
        public void GetService_CalledOnServiceProvider_ReturnsExpected()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<IEmbeddingGenerator>();

            // Assert
            Assert.NotNull(service);
            Assert.Equal(mockGenerator.Object, service);
        }
    }
}
