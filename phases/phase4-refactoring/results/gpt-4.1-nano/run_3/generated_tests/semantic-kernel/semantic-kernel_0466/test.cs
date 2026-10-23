using Xunit;
using Moq;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;

namespace VectorData.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenGeneratorIsRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var generatorMock = new Mock<IEmbeddingGenerator>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                               .Returns(generatorMock.Object);

            var options = new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generatorMock.Object, result?.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                               .Returns(null);

            var options = new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result?.EmbeddingGenerator);
            Assert.Equal(options, result);
        }
    }
}
