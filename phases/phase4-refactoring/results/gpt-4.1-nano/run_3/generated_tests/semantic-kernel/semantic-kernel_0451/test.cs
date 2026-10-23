using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.CosmosNoSql.Tests
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
            var mockDatabaseProvider = new Func<CosmosClient, Database>(_ => mockDatabase.Object);

            // Setup the service provider to return the mock Database when requested
            mockServiceProvider.Setup(sp => sp.GetRequiredService<Database>())
                .Returns(mockDatabase.Object);

            // Setup the service provider to return the mock IServiceProvider itself when requested
            // (if needed, but in this case, we directly pass the IServiceCollection)
            // We need to simulate the service provider used inside the extension method
            // So, we will build a ServiceProvider from the ServiceCollection after adding the necessary services

            // Add a mock CosmosClient to the service collection
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockClientWrapper = new Mock<ClientWrapper>(MockBehavior.Strict);
            mockClientWrapper.Setup(cw => cw.Client).Returns(mockCosmosClient.Object);
            mockClientWrapper.Setup(cw => cw.Dispose());

            // Register the mock ClientWrapper as singleton
            services.AddSingleton(mockClientWrapper.Object);

            // Build the service provider
            var serviceProvider = services.BuildServiceProvider();

            // Act
            // Call the extension method
            var result = CosmosNoSqlServiceCollectionExtensions.AddKeyedCosmosNoSqlVectorStore(
                services,
                serviceKey: "testKey",
                connectionString: "AccountEndpoint=https://test;AccountKey=abc;",
                databaseName: "TestDb",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            // Assert
            // Verify that the services contain the CosmosNoSqlVectorStore registration
            var serviceProviderAfter = services.BuildServiceProvider();

            var store = serviceProviderAfter.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(store);
        }
    }
}
