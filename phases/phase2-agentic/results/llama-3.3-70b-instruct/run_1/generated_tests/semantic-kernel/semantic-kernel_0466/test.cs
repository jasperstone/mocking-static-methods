using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ServiceProviderHasEmbeddingGenerator_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator>(Mock.Of<IEmbeddingGenerator>())
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ServiceProviderDoesNotHaveEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions();

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.Null(options.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsNull_ReturnsNull()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => null;

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.Null(options);
        }

        [Fact]
        public void GetStoreOptions_OptionsProviderReturnsOptionsWithEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = (IServiceProvider sp) => new PostgresVectorStoreOptions { EmbeddingGenerator = Mock.Of<IEmbeddingGenerator>() };

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.NotNull(options);
            Assert.NotNull(options.EmbeddingGenerator);
        }
    }
}
