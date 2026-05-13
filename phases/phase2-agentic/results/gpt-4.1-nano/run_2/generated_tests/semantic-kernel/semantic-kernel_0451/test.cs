using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.VectorData.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Call_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockServiceProvider = new Mock<IServiceProvider>();

            // Setup the service provider to return the mock Database when requested
            mockServiceProvider.Setup(sp => sp.GetRequiredService<Database>())
                .Returns(mockDatabase.Object);

            // Register the mock service provider in the service collection
            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(
                services,
                serviceKey: "testKey",
                connectionString: "AccountEndpoint=https://test;AccountKey=key;",
                databaseName: "TestDb",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve the CosmosNoSqlVectorStore to trigger the factory
            var store = serviceProvider.GetService<VectorStore>();

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<Database>(), Times.AtLeastOnce);
            Assert.NotNull(store);
        }
    }
}
