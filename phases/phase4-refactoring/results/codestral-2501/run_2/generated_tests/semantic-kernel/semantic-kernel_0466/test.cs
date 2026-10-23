using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using System;
using System.Reflection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresVectorStoreOptions();

            // Act
            var result = InvokePrivateMethod<PostgresVectorStoreOptions?>("GetStoreOptions", serviceProviderMock.Object, new Func<IServiceProvider, PostgresVectorStoreOptions?>(_ => options));

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var result = InvokePrivateMethod<PostgresVectorStoreOptions?>("GetStoreOptions", serviceProviderMock.Object, new Func<IServiceProvider, PostgresVectorStoreOptions?>(_ => options));

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnNull_WhenOptionsProviderIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokePrivateMethod<PostgresVectorStoreOptions?>("GetStoreOptions", serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresCollectionOptions();

            // Act
            var result = InvokePrivateMethod<PostgresCollectionOptions?>("GetCollectionOptions", serviceProviderMock.Object, new Func<IServiceProvider, PostgresCollectionOptions?>(_ => options));

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var result = InvokePrivateMethod<PostgresCollectionOptions?>("GetCollectionOptions", serviceProviderMock.Object, new Func<IServiceProvider, PostgresCollectionOptions?>(_ => options));

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnNull_WhenOptionsProviderIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokePrivateMethod<PostgresCollectionOptions?>("GetCollectionOptions", serviceProviderMock.Object, null);

            // Assert
            Assert.Null(result);
        }

        private T InvokePrivateMethod<T>(string methodName, params object[] parameters)
        {
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            return (T)method.Invoke(null, parameters);
        }
    }
}
