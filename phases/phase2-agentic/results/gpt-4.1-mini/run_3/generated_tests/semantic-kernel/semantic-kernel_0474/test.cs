using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_UsesEmbeddingGeneratorFromServiceProvider_WhenOptionsEmbeddingGeneratorIsNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions();
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotSame(options, result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsDirectly_WhenOptionsEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);

            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsDirectly_WhenEmbeddingGeneratorFromServiceProviderIsNull()
        {
            // Arrange
            var options = new QdrantVectorStoreOptions();
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(_ => options);

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderIsNullAndNoEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(null);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenOptionsProviderIsNullButEmbeddingGeneratorExists()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                .Returns(embeddingGeneratorMock.Object);

            // Act
            var result = InvokeGetStoreOptions(serviceProviderMock.Object, null);

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
