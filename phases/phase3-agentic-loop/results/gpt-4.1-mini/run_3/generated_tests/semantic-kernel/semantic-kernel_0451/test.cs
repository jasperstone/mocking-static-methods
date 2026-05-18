using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Microsoft.Azure.Cosmos;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndFactoryResolvesCosmosNoSqlVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add a mock Database service to satisfy GetRequiredService<Database>()
            var mockDatabase = new MockDatabase();
            services.AddSingleton<Database>(mockDatabase);

            // Act
            var returnedServices = services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null);

            // Assert
            Assert.Same(services, returnedServices);

            // Check that CosmosNoSqlVectorStore and VectorStore are registered
            var cosmosDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(CosmosNoSqlVectorStore));
            Assert.NotNull(cosmosDescriptor);
            Assert.NotNull(cosmosDescriptor.ImplementationFactory);

            var vectorStoreDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(VectorStore));
            Assert.NotNull(vectorStoreDescriptor);
            Assert.NotNull(vectorStoreDescriptor.ImplementationFactory);

            // Build provider to test the factory delegate calls GetRequiredService<Database>()
            var provider = services.BuildServiceProvider();

            // Invoke the factory delegate for CosmosNoSqlVectorStore
            var cosmosStore = cosmosDescriptor.ImplementationFactory!(provider, null);
            Assert.NotNull(cosmosStore);
            Assert.IsType<CosmosNoSqlVectorStore>(cosmosStore);

            // Invoke the factory delegate for VectorStore
            var vectorStore = vectorStoreDescriptor.ImplementationFactory!(provider, null);
            Assert.NotNull(vectorStore);
            // The VectorStore registration returns CosmosNoSqlVectorStore keyed service, so it is CosmosNoSqlVectorStore
            Assert.IsType<CosmosNoSqlVectorStore>(vectorStore);
        }

        // Minimal mock Database class to satisfy GetRequiredService<Database>()
        private class MockDatabase : Database
        {
            public MockDatabase() : base(null, null, null) { }
        }
    }
}
