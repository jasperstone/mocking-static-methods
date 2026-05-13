using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    [TestClass]
    public class QdrantServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void GetStoreOptions_ServiceProviderWithEmbeddingGenerator_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => null);

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.EmbeddingGenerator);
        }

        [TestMethod]
        public void GetStoreOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantVectorStoreOptions?>(sp => new QdrantVectorStoreOptions());

            // Act
            var options = QdrantServiceCollectionExtensions.GetStoreOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNull(options.EmbeddingGenerator);
        }

        [TestMethod]
        public void GetCollectionOptions_ServiceProviderWithEmbeddingGenerator_ReturnsNewOptionsWithEmbeddingGenerator()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IEmbeddingGenerator, MockEmbeddingGenerator>()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => null);

            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNotNull(options.EmbeddingGenerator);
        }

        [TestMethod]
        public void GetCollectionOptions_ServiceProviderWithoutEmbeddingGenerator_ReturnsOriginalOptions()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var optionsProvider = new Func<IServiceProvider, QdrantCollectionOptions?>(sp => new QdrantCollectionOptions());

            // Act
            var options = QdrantServiceCollectionExtensions.GetCollectionOptions(serviceProvider, optionsProvider);

            // Assert
            Assert.IsNotNull(options);
            Assert.IsNull(options.EmbeddingGenerator);
        }
    }

    public class MockEmbeddingGenerator : IEmbeddingGenerator
    {
        public void GenerateEmbedding(object input)
        {
            throw new NotImplementedException();
        }
    }
}
