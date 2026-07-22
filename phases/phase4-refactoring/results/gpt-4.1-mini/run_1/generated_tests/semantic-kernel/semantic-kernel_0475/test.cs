using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var mockSp = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>();
            var optionsWithEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = mockEmbeddingGenerator.Object };

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithEmbedding;

            // Act
            var result = InvokeGetCollectionOptions(mockSp.Object, optionsProvider);

            // Assert
            Assert.Same(optionsWithEmbedding, result);
            mockSp.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator)), Times.Never);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var mockSp = new Mock<IServiceProvider>();
            mockSp.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(null);
            var optionsWithoutEmbedding = new QdrantCollectionOptions { EmbeddingGenerator = null };

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => optionsWithoutEmbedding;

            // Act
            var result = InvokeGetCollectionOptions(mockSp.Object, optionsProvider);

            // Assert
            Assert.Same(optionsWithoutEmbedding, result);
            mockSp.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var mockSp = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>();
            mockSp.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);
            var originalOptions = new QdrantCollectionOptions { EmbeddingGenerator = null };

            Func<IServiceProvider, QdrantCollectionOptions?> optionsProvider = _ => originalOptions;

            // Act
            var result = InvokeGetCollectionOptions(mockSp.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(originalOptions, result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
            mockSp.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNull_WhenOptionsProviderIsNull_AndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var mockSp = new Mock<IServiceProvider>();
            mockSp.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(null);

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            // Act
            var result = InvokeGetCollectionOptions(mockSp.Object, optionsProvider);

            // Assert
            Assert.Null(result);
            mockSp.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNull_AndEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var mockSp = new Mock<IServiceProvider>();
            var mockEmbeddingGenerator = new Mock<Microsoft.Extensions.AI.IEmbeddingGenerator>();
            mockSp.Setup(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator))).Returns(mockEmbeddingGenerator.Object);

            Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider = null;

            // Act
            var result = InvokeGetCollectionOptions(mockSp.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(mockEmbeddingGenerator.Object, result.EmbeddingGenerator);
            mockSp.Verify(sp => sp.GetService(typeof(Microsoft.Extensions.AI.IEmbeddingGenerator)), Times.Once);
        }

        private static QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, QdrantCollectionOptions?>? optionsProvider)
        {
            var type = typeof(QdrantServiceCollectionExtensions);
            var method = type.GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantCollectionOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
