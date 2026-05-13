using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndCallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            // Register a mock Database service to satisfy GetRequiredService<Database>()
            var mockDatabase = new Database();
            services.AddSingleton(mockDatabase);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Assert
            // The CosmosNoSqlVectorStore service should be resolvable and use the registered Database instance
            var cosmosStore = provider.GetRequiredService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosStore);

            // The VectorStore service should be resolvable and be the same instance as CosmosNoSqlVectorStore
            var vectorStore = provider.GetRequiredService<VectorStore>();
            Assert.NotNull(vectorStore);
            Assert.Same(cosmosStore, vectorStore);
        }
    }
}
