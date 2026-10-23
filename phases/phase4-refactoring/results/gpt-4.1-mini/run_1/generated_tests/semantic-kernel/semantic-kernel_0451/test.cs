using System;
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
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndResolvesDatabase()
        {
            // Arrange
            var services = new ServiceCollection();

            // Create a mock Database to satisfy the GetRequiredService<Database>() call
            var mockDatabase = new Mock<Database>(MockBehavior.Strict, null, null);
            services.AddSingleton<Database>(mockDatabase.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            // Build the service provider
            var provider = services.BuildServiceProvider();

            // Assert
            // The CosmosNoSqlVectorStore service should be resolvable and not null
            var cosmosStore = provider.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosStore);

            // The VectorStore service should be resolvable and be the same instance as CosmosNoSqlVectorStore
            var vectorStore = provider.GetService<VectorStore>();
            Assert.NotNull(vectorStore);
            Assert.Same(cosmosStore, vectorStore);
        }
    }
}
