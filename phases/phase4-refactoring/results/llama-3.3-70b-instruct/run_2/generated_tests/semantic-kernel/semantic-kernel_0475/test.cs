using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Qdrant.Client;
using System;
using System.Threading;
using Xunit;

namespace Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(Mock.Of<IEmbeddingGenerator>())
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantVectorStoreOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ServiceProviderWithEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(Mock.Of<IEmbeddingGenerator>())
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantCollectionOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new QdrantCollectionOptions();

            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }
    }
}
