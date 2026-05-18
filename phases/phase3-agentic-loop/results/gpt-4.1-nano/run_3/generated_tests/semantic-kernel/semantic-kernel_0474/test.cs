using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace VectorData.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProvidesGenerator()
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
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenServiceProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_CreatesNewOptionsWithGenerator_WhenGeneratorServiceExists()
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
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenServiceProvidesGenerator()
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
            Assert.Equal(mockGenerator.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenServiceProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_CreatesNewOptionsWithGenerator_WhenGeneratorServiceExists()
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

        [Fact]
        public void AddKeyedQdrantCollection_RegistersServices_CallsGetServiceOnProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object };
            mockProvider.Setup(p => p.GetService(typeof(IEmbeddingGenerator))).Returns(mockGenerator.Object);
            mockProvider.Setup(p => p.GetService(typeof(QdrantCollectionOptions))).Returns(options);

            // Act
            var result = QdrantServiceCollectionExtensions.AddKeyedQdrantCollection<string, object>(
                services,
                "test",
                "host",
                clientProvider: null,
                optionsProvider: sp => null,
                ServiceLifetime.Singleton);

            // Assert
            Assert.NotNull(result);
            var serviceProvider = result.BuildServiceProvider();
            var store = serviceProvider.GetService<QdrantVectorStore>();
            Assert.NotNull(store);
        }
    }
}
