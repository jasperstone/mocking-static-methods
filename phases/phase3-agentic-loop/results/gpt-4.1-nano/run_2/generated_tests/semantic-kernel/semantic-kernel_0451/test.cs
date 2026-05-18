using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using Moq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace CosmosNoSqlTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Register_CosmosNoSqlVectorStore_And_VectorStore()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<Database>()).Returns(mockDatabase.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(
                serviceKey: "testKey",
                connectionString: "AccountEndpoint=https://test;AccountKey=key;",
                databaseName: "testDb",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Retrieve the CosmosNoSqlVectorStore
            var store = provider.GetService<CosmosNoSqlVectorStore>();
            var vectorStore = provider.GetService<VectorStore>();

            // Assert
            Assert.NotNull(store);
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Call_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<Database>()).Returns(mockDatabase.Object);

            // Setup a service descriptor with a factory that calls GetRequiredService<Database>
            services.Add(new ServiceDescriptor(typeof(CosmosNoSqlVectorStore), "testKey", (sp, _) =>
            {
                var database = sp.GetRequiredService<Database>();
                return new CosmosNoSqlVectorStore(database, null);
            }, ServiceLifetime.Singleton));

            var provider = services.BuildServiceProvider();

            // Act
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(CosmosNoSqlVectorStore));
            Assert.NotNull(descriptor);

            // Create a scope to simulate service resolution
            using var scope = provider.CreateScope();
            var sp = scope.ServiceProvider;

            // Act: resolve the factory delegate
            var factory = descriptor.ImplementationInstance ?? throw new Exception("Factory delegate not found");
            var store = factory(sp, null);

            // Assert
            Assert.IsType<CosmosNoSqlVectorStore>(store);
            mockDatabase.Verify(db => db, Times.AtLeastOnce);
        }
    }
}
