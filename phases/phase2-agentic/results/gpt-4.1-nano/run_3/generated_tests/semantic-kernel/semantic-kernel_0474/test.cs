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
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProviderHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithoutModification_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithGenerator_WhenServiceProviderHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => new QdrantVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
            Assert.NotSame(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProviderHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithoutModification_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithGenerator_WhenServiceProviderHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
            Assert.NotSame(options, result);
        }

        [Fact]
        public void GetService_ReturnsNull_WhenServiceNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var service = provider.GetService<IEmbeddingGenerator>();

            // Assert
            Assert.Null(service);
        }

        [Fact]
        public void GetService_ReturnsService_WhenRegistered()
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
