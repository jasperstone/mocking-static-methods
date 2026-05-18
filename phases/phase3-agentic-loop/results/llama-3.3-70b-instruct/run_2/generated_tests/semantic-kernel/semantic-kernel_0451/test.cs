using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace VectorDataTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCosmosNoSqlVectorStore_WithDatabaseService_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddCosmosNoSqlVectorStore_WithConnectionStringAndDatabaseName_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var cosmosClientMock = new Mock<CosmosClient>("connectionString");
            services.AddSingleton<CosmosClient>(cosmosClientMock.Object);

            // Act
            services.AddCosmosNoSqlVectorStore("connectionString", "databaseName");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_WithDatabaseService_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore("key");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_WithConnectionStringAndDatabaseName_AddsVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var cosmosClientMock = new Mock<CosmosClient>("connectionString");
            services.AddSingleton<CosmosClient>(cosmosClientMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore("key", "connectionString", "databaseName");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
        }
    }
}
