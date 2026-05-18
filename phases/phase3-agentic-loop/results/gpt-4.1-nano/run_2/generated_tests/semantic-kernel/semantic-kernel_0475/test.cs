using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace VectorData.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        private class DummyEmbeddingGenerator : IEmbeddingGenerator { }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new DummyEmbeddingGenerator() };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGeneratorFromService_WhenOptionsProviderNullAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenNoGeneratorAndServiceHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = new DummyEmbeddingGenerator() };
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(_ => options);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGeneratorFromService_WhenOptionsProviderNullAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenNoGeneratorAndServiceHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenOptionsProviderNullAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithoutModification_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = new DummyEmbeddingGenerator() };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGenerator_WhenOptionsProviderNullAndServiceHasGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddTransient(_ => mockGenerator.Object);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }
    }
}
