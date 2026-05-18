using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.PgVector; // Assuming this is where PostgresCollectionOptions is defined

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenServiceProviderReturnsEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = (IServiceProvider sp) => new PostgresCollectionOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOriginalOptions_WhenServiceProviderReturnsNullEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            var optionsProvider = (IServiceProvider sp) => new PostgresCollectionOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithProvidedEmbeddingGenerator_WhenOptionsAlreadyHaveEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns<IEmbeddingGenerator>(null);

            var optionsProvider = (IServiceProvider sp) => new PostgresCollectionOptions
            {
                EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object
            };

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.EmbeddingGenerator);
        }
    }
}
