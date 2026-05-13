using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class PostgresServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void GetStoreOptions_ServiceProviderWithIEmbeddingGenerator_ReturnsOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, PostgresVectorStoreOptions?>(sp => null);

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.EmbeddingGenerator);
        }

        [TestMethod]
        public void GetStoreOptions_ServiceProviderWithoutIEmbeddingGenerator_ReturnsOptionsWithoutEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, PostgresVectorStoreOptions?>(sp => null);

            // Act
            var options = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNull(options.EmbeddingGenerator);
        }

        private class MockEmbeddingGenerator : IEmbeddingGenerator
        {
            public void GenerateEmbedding(object input)
            {
                throw new NotImplementedException();
            }
        }
    }
}
