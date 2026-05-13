using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersCosmosNoSqlVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, null, ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosNoSqlVectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, null, ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ThrowsException_WhenDatabaseIsNotRegistered()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => services.AddKeyedCosmosNoSqlVectorStore(null, null, ServiceLifetime.Singleton));
        }
    }
}
