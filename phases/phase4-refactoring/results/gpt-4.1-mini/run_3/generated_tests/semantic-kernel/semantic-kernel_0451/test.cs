using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ResolvesCosmosNoSqlVectorStore_WithRegisteredDatabase()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a CosmosClient with a dummy connection string (will not connect)
            var cosmosClient = new CosmosClient("AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM==;");
            var database = cosmosClient.GetDatabase("TestDatabase");

            // Register the Database instance
            services.AddSingleton<Database>(database);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var vectorStore = provider.GetRequiredService<CosmosNoSqlVectorStore>();
            Assert.NotNull(vectorStore);
        }
    }
}
