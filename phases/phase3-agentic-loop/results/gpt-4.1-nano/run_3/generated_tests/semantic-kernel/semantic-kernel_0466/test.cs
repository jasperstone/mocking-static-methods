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
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetStoreOptions_ReturnsOptionsWithGenerator_WhenGeneratorIsAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>().Object;
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator);
            var options = new PostgresVectorStoreOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator, result!.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithEmbeddedGenerator_WhenGeneratorIsPresent()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new PostgresCollectionOptions { EmbeddingGenerator = new Mock<IEmbeddingGenerator>().Object };
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(provider, sp => options);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(options, result);
        }

        [Fact]
        public void GetCollectionOptions_ReturnsOptionsWithGenerator_WhenGeneratorIsAvailable()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockGenerator = new Mock<IEmbeddingGenerator>().Object;
            services.AddSingleton<IEmbeddingGenerator>(mockGenerator);
            var options = new PostgresCollectionOptions();
            services.AddSingleton(options);
            var provider = services.BuildServiceProvider();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(provider, sp => null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockGenerator, result!.EmbeddingGenerator);
        }
    }
}
