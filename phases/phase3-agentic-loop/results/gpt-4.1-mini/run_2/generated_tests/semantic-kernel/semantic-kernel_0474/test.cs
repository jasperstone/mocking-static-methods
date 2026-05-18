using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Qdrant.Client;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var spMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenNoEmbeddingGeneratorAndNoService()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenNoEmbeddingGeneratorInOptionsButServiceExists()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderIsNullAndNoService()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNullButServiceExists()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
