using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock };
            var spMock = new Mock<IServiceProvider>();
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorNotInServiceProviderAndOptionsNull()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => null;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsNullAndEmbeddingGeneratorFromServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock);
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => null;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsWithoutEmbeddingGeneratorAndEmbeddingGeneratorFromServiceProvider()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>().Object;
            var options = new QdrantVectorStoreOptions();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock);
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock, result.EmbeddingGenerator);
        }

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
