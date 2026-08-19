using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCosmosNoSqlVectorStoreRegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<Database>(new Mock<Database>().Object);

            // Act
            services.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService(typeof(CosmosNoSqlVectorStore)) as CosmosNoSqlVectorStore;
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStoreRegistersService()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<Database>(new Mock<Database>().Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore("key");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var vectorStore = serviceProvider.GetService(typeof(CosmosNoSqlVectorStore), "key") as CosmosNoSqlVectorStore;
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddCosmosNoSqlVectorStoreRegistersDatabase()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var databaseMock = new Mock<Database>();
            services.TryAddSingleton(databaseMock.Object);

            // Act
            services.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var database = serviceProvider.GetService(typeof(Database)) as Database;
            Assert.NotNull(database);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStoreRegistersDatabase()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            var databaseMock = new Mock<Database>();
            services.TryAddSingleton(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore("key");

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var database = serviceProvider.GetService(typeof(Database)) as Database;
            Assert.NotNull(database);
        }
    }
}
