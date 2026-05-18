using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Moq;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class QdrantServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.NotNull(result);
            var options = result as QdrantCollectionOptions;
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => options);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.NotNull(result);
            var returnedOptions = result as QdrantCollectionOptions;
            Assert.Same(embeddingGeneratorMock.Object, returnedOptions.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnNull_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetCollectionOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => null);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.NotNull(result);
            var options = result as QdrantVectorStoreOptions;
            Assert.Same(embeddingGeneratorMock.Object, options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new QdrantVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => options);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.NotNull(result);
            var returnedOptions = result as QdrantVectorStoreOptions;
            Assert.Same(embeddingGeneratorMock.Object, returnedOptions.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnNull_WhenOptionsProviderReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => null);

            // Act
            var method = typeof(QdrantServiceCollectionExtensions).GetMethod("GetStoreOptions", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { serviceProviderMock.Object, optionsProvider });

            // Assert
            Assert.Null(result);
        }
    }
}
