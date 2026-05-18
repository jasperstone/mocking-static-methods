using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetService_ShouldReturnNull_WhenServiceNotRegistered()
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
        public void GetService_ShouldReturnService_WhenServiceRegistered()
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
            Assert.IsType<Mock<IEmbeddingGenerator>>().IsAssignableFrom(service.GetType());
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddedGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddedGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator.Object);
            var provider = services.BuildServiceProvider();

            var options = new QdrantCollectionOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithoutModification_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var options = new QdrantVectorStoreOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithoutModification_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var provider = services.BuildServiceProvider();

            var options = new QdrantCollectionOptions();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result?.EmbeddingGenerator);
        }
    }
}
