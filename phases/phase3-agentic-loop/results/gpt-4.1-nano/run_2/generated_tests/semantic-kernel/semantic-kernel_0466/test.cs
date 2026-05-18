using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;

namespace PostgresServiceCollectionExtensionsTests
{
    public class GetStoreOptionsTests
    {
        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorIsPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNewOptionsWithGenerator_WhenGeneratorIsPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            var generator = new Mock<IEmbeddingGenerator>().Object;
            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = generator };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generator, result.EmbeddingGenerator);
            Assert.NotSame(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOriginalOptions_WhenGeneratorIsNull()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new PostgresVectorStoreOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsNull_WhenOptionsProviderReturnsNullAndNoGenerator()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new PostgresVectorStoreOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.EmbeddingGenerator);
        }
    }
}
