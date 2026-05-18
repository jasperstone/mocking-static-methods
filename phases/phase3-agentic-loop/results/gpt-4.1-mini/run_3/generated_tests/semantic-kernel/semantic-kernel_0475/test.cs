using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.AI;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var sp = new Mock<IServiceProvider>(MockBehavior.Strict);
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGenerator };
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(sp.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            sp.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Never);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndServiceProviderReturnsNull()
        {
            // Arrange
            var sp = new Mock<IServiceProvider>();
            sp.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetStoreOptions(sp.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            sp.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsNull_AndServiceProviderReturnsEmbeddingGenerator()
        {
            // Arrange
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var sp = new Mock<IServiceProvider>();
            sp.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator);
            Func<IServiceProvider, QdrantVectorStoreOptions?> optionsProvider = _ => null;

            // Act
            var result = InvokeGetStoreOptions(sp.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var sp = new Mock<IServiceProvider>(MockBehavior.Strict);
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var options = new Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions { EmbeddingGenerator = embeddingGenerator };
            Func<IServiceProvider, Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetCollectionOptions(sp.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            sp.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Never);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptions_WhenOptionsProviderReturnsOptionsWithoutEmbeddingGenerator_AndServiceProviderReturnsNull()
        {
            // Arrange
            var sp = new Mock<IServiceProvider>();
            sp.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);
            var options = new Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions { EmbeddingGenerator = null };
            Func<IServiceProvider, Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions?> optionsProvider = _ => options;

            // Act
            var result = InvokeGetCollectionOptions(sp.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
            sp.Verify(sp => sp.GetService(typeof(IEmbeddingGenerator)), Times.Once);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderReturnsNull_AndServiceProviderReturnsEmbeddingGenerator()
        {
            // Arrange
            var embeddingGenerator = new Mock<IEmbeddingGenerator>().Object;
            var sp = new Mock<IServiceProvider>();
            sp.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGenerator);
            Func<IServiceProvider, Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions?> optionsProvider = _ => null;

            // Act
            var result = InvokeGetCollectionOptions(sp.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGenerator, result.EmbeddingGenerator);
        }

        // Helper methods to invoke private static methods via reflection
        private static QdrantVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, QdrantVectorStoreOptions?>? optionsProvider)
        {
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (QdrantVectorStoreOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }

        private static Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions? InvokeGetCollectionOptions(IServiceProvider sp, Func<IServiceProvider, Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions?>? optionsProvider)
        {
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            return (Microsoft.SemanticKernel.Connectors.Qdrant.QdrantCollectionOptions?)method!.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
