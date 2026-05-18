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
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndResolvesDatabase()
        {
            // Arrange
            var services = new ServiceCollection();

            // We need to register a Database instance for the GetRequiredService<Database>() call
            var cosmosClient = new CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4Q==");
            var database = cosmosClient.GetDatabase("TestDatabase");
            services.AddSingleton(database);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            var provider = services.BuildServiceProvider();

            // Assert
            // The CosmosNoSqlVectorStore should be resolvable and its Database dependency should be the registered one
            var vectorStore = provider.GetRequiredService<CosmosNoSqlVectorStore>();
            Assert.NotNull(vectorStore);

            // We cannot directly access the Database property on CosmosNoSqlVectorStore (it's private),
            // but if the service was constructed, the call to GetRequiredService<Database>() succeeded.
            // So this test ensures the extension method calls GetRequiredService on IServiceProvider as expected.
        }
    }
}
