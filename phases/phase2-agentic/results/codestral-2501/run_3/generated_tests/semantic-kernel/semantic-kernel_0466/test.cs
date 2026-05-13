using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Npgsql;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PostgresServiceCollectionExtensionsTests
    {
        [Fact]
        public void GetStoreOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresVectorStoreOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetStoreOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresVectorStoreOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var result = PostgresServiceCollectionExtensions.GetStoreOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptionsWithEmbeddingGenerator_WhenEmbeddingGeneratorIsNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresCollectionOptions();

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void GetCollectionOptions_ShouldReturnOptions_WhenEmbeddingGeneratorIsNotNull()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var embeddingGeneratorMock = new Mock<IEmbeddingGenerator>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(IEmbeddingGenerator))).Returns(embeddingGeneratorMock.Object);

            var options = new PostgresCollectionOptions { EmbeddingGenerator = embeddingGeneratorMock.Object };

            // Act
            var result = PostgresServiceCollectionExtensions.GetCollectionOptions(serviceProviderMock.Object, _ => options);

            // Assert
            Assert.NotNull(result);
            Assert.Same(embeddingGeneratorMock.Object, result.EmbeddingGenerator);
        }

        [Fact]
        public void AddVectorStore_ShouldAddServices_WhenCalled()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceKey = new object();
            var connectionString = "connectionString";
            var options = new PostgresVectorStoreOptions();
            var lifetime = ServiceLifetime.Singleton;

            // Act
            var result = PostgresServiceCollectionExtensions.AddVectorStore(serviceCollection, serviceKey, sp => connectionString, sp => options, lifetime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, sd => sd.ServiceType == typeof(PostgresVectorStore) && sd.Lifetime == lifetime);
            Assert.Contains(result, sd => sd.ServiceType == typeof(VectorStore) && sd.Lifetime == lifetime);
        }

        [Fact]
        public void AddAbstractions_ShouldAddServices_WhenCalled()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceKey = new object();
            var lifetime = ServiceLifetime.Singleton;

            // Act
            PostgresServiceCollectionExtensions.AddAbstractions<string, object>(serviceCollection, serviceKey, lifetime);

            // Assert
            Assert.Equal(2, serviceCollection.Count);
            Assert.Contains(serviceCollection, sd => sd.ServiceType == typeof(VectorStoreCollection<string, object>) && sd.Lifetime == lifetime);
            Assert.Contains(serviceCollection, sd => sd.ServiceType == typeof(IVectorSearchable<object>) && sd.Lifetime == lifetime);
        }
    }
}
