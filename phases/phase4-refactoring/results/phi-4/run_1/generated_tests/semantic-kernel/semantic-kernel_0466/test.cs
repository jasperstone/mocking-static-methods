using Moq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorIsAvailable_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                               .Returns(embeddingGeneratorMock.Object);

            var optionsProvider = new Mock<Func<IServiceProvider, PostgresVectorStoreOptions?>>();
            optionsProvider.Setup(p => p.Invoke(It.IsAny<IServiceProvider>()))
                           .Returns(new PostgresVectorStoreOptions());

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_WhenEmbeddingGeneratorIsNotAvailable_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator)))
                               .Returns((IEmbeddingGenerator)null);

            var optionsProvider = new Mock<Func<IServiceProvider, PostgresVectorStoreOptions?>>();
            var originalOptions = new PostgresVectorStoreOptions();
            optionsProvider.Setup(p => p.Invoke(It.IsAny<IServiceProvider>()))
                           .Returns(originalOptions);

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, optionsProvider.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Same(originalOptions, result);
        }
    }
}
