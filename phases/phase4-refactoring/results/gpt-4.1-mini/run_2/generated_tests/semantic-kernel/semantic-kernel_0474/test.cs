using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_OptionsWithEmbeddingGenerator_ReturnsSameOptions()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock };
            var spMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_OptionsWithoutEmbeddingGenerator_EmbeddingGeneratorFromServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsNull_EmbeddingGeneratorFromServiceProvider_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsNull_EmbeddingGeneratorNotInServiceProvider_ReturnsNull()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_OptionsWithoutEmbeddingGenerator_EmbeddingGeneratorNotInServiceProvider_ReturnsOptions()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
