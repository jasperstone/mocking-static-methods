using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorNotProvided_ShouldRetrieveFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService<IEmbeddingGenerator>()).Returns(embeddingGeneratorMock.Object);

            var optionsProvider = (IServiceProvider sp) => new PostgresCollectionOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<IEmbeddingGenerator>(), Times.Once);
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_WhenEmbeddingGeneratorProvided_ShouldReturnOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var options = new PostgresCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };

            var optionsProvider = (IServiceProvider sp) => options;

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, optionsProvider);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService<IEmbeddingGenerator>(), Times.Never);
            Assert.Same(options, result);
        }
    }
