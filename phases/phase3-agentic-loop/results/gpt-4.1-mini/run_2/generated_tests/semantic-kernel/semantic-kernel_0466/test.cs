using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenOptionsHasEmbeddingGenerator()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };
            var spMock = new Mock<IServiceProvider>();

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptions_WhenEmbeddingGeneratorServiceNotRegistered()
        {
            // Arrange
            var options = new PostgresVectorStoreOptions();
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => options);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorServiceRegistered()
        {
            // Arrange
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            var options = new PostgresVectorStoreOptions();
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
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderReturnsNullAndNoEmbeddingGenerator()
        {
            // Arrange
            var spMock = new Mock<IServiceProvider>();
            spMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(null);

            // Act
            var result = InvokeGetStoreOptions(spMock.Object, _ => null);

            // Assert
            Assert.Null(result);
        }

        private static PostgresVectorStoreOptions? InvokeGetStoreOptions(IServiceProvider sp, Func<IServiceProvider, PostgresVectorStoreOptions?>? optionsProvider)
        {
            // Use reflection to invoke the private static method GetStoreOptions
            var method = typeof(PostgresServiceCollectionExtensions).GetMethod("GetStoreOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.NotNull(method);
            return (PostgresVectorStoreOptions?)method.Invoke(null, new object?[] { sp, optionsProvider });
        }
    }
}
