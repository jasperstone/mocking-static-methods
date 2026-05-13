using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        private class DummyOptions : QdrantVectorStoreOptions
        {
            public IEmbeddingGenerator? EmbeddingGenerator { get; set; }
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new DummyOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsUnchanged_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DummyOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenServiceProviderHasGenerator()
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
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenNoGeneratorAndServiceProviderHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DummyOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddingGenerator_WhenOptionsProviderProvidesGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new DummyOptions { EmbeddingGenerator = mockGenerator.Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsUnchanged_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DummyOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGenerator_WhenServiceProviderHasGenerator()
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
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenNoGeneratorAndServiceProviderHasNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new DummyOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.Null(result?.EmbeddingGenerator);
        }
    }
}
