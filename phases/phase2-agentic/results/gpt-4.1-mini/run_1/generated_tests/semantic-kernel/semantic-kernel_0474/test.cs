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
        // We test the private static method GetStoreOptions via reflection
        // because it is private and static.
        // The key line to cover is the call to sp.GetService<IEmbeddingGenerator>().

        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (QdrantVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsDirectly_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var spMock = new Mock<IServiceProvider>();

            // optionsProvider returns options with EmbeddingGenerator set
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            // The IServiceProvider.GetService should not be called in this case
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Never);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsIsNullAndNoEmbeddingGeneratorInServiceProvider()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => null;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.Null(result);
            spMock.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsHasNoEmbeddingGeneratorButServiceProviderHas()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var originalOptions = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => originalOptions;

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(originalOptions, result);
            Assert.Same(embeddingGeneratorMock.Object, result!.EmbeddingGenerator);
        }
    }
}
