using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;

namespace CosmosNoSqlTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Call_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockCosmosClient = new Mock<CosmosClient>();
            mockCosmosClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(mockDatabase.Object);

            var mockClientWrapper = new Mock<ClientWrapper>(mockCosmosClient.Object, false);
            mockClientWrapper.Setup(cw => cw.Share()).Returns(mockClientWrapper.Object);
            mockClientWrapper.Setup(cw => cw.Dispose());

            // Register the mock ClientWrapper as singleton
            services.AddSingleton(mockClientWrapper.Object);

            // Register a dummy IServiceProvider that returns the mock database
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<Database>()).Returns(mockDatabase.Object);
            services.AddSingleton<IServiceProvider>(mockServiceProvider.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(
                serviceKey: "testKey",
                connectionString: "AccountEndpoint=https://test;AccountKey=abc;",
                databaseName: "TestDb",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            var provider = services.BuildServiceProvider();

            // Trigger the lambda to get the store
            var store = provider.GetService<CosmosNoSqlVectorStore>();

            // Assert
            Assert.NotNull(store);
            mockServiceProvider.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }
    }
}
