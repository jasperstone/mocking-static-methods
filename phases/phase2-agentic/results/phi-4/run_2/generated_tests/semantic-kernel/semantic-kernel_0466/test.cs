using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldRetrieveEmbeddingGeneratorFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnProvidedOptionsIfEmbeddingGeneratorIsSet()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

            var optionsProvider = (IServiceProvider sp) => options;

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.Same(options, result);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnNullEmbeddingGeneratorIfServiceProviderReturnsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns((IEmbeddingGenerator)null);

            var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }
    }
}
