using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseService_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, null, ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseServiceAndOptions_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);
            var options = new CosmosNoSqlVectorStoreOptions();

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, options, ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions());
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_WithoutDatabaseService_ThrowsException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddKeyedCosmosNoSqlVectorStore(null, null, ServiceLifetime.Singleton));
        }
    }
}
