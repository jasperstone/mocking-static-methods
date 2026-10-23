using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Call_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var databaseInstance = mockDatabase.Object;

            // Setup a service provider that returns the mock Database
            var serviceProvider = new ServiceCollection()
                .AddSingleton(databaseInstance)
                .BuildServiceProvider();

            // Add the service provider to the services
            services.AddSingleton(serviceProvider);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(
                serviceKey: "testKey",
                connectionString: "dummy",
                databaseName: "db",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Retrieve the CosmosNoSqlVectorStore
            var store = provider.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(store);

            // Verify that the database was resolved during registration
            // Since the constructor calls GetRequiredService<Database>(), the mock should be used
            // We can check that the store's internal database matches our mock
            var internalDatabaseField = typeof(CosmosNoSqlVectorStore)
                .GetField("_database", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var internalDatabase = internalDatabaseField?.GetValue(store);
            Assert.Equal(databaseInstance, internalDatabase);
        }
    }
}
