using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = mockGenerator.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithGenerator_WhenOptionsWithoutGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions(); // no generator
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);
            var spMock = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            spMock.Setup(s => s.GetService(typeof(IEmbeddingGenerator))).Returns(mockGenerator.Object);
            var sp = spMock.Object;

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantVectorStoreOptions(); // no generator
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);
            var sp = new ServiceCollection().BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenProvided()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = mockGenerator.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(_ => options);
            var sp = services.BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithGenerator_WhenOptionsWithoutGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions(); // no generator
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(_ => options);
            var spMock = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            spMock.Setup(s => s.GetService(typeof(IEmbeddingGenerator))).Returns(mockGenerator.Object);
            var sp = spMock.Object;

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator.Object, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new QdrantCollectionOptions(); // no generator
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(_ => options);
            var sp = new ServiceCollection().BuildServiceProvider();

            // Act
            var result = QdrantServiceCollectionExtensions.GetCollectionOptions(sp, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_CallsGetServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockProvider = new Mock<IServiceProvider>();
            var mockGenerator = new Mock<IEmbeddingGenerator>();
            mockProvider.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(mockGenerator.Object);

            // Act
            var options = new QdrantVectorStoreOptions();
            var result = QdrantServiceCollectionExtensions.GetStoreOptions(mockProvider.Object, _ => options);

            // Assert
            mockProvider.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
            Assert.NotNull(result);
        }
    }
}
