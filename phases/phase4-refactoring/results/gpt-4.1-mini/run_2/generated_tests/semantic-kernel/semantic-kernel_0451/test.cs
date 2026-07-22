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
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndResolvesVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a CosmosClient with a dummy connection string
            var dummyConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM==;";
            var cosmosClient = new CosmosClient(dummyConnectionString);
            services.AddSingleton(cosmosClient);

            // Register Database service using the CosmosClient
            services.AddSingleton(sp => sp.GetRequiredService<CosmosClient>().GetDatabase("TestDatabase"));

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            var provider = services.BuildServiceProvider();

            // Assert
            var cosmosStore = provider.GetRequiredService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosStore);

            var vectorStore = provider.GetRequiredService<VectorStore>();
            Assert.NotNull(vectorStore);
        }
    }
}
