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
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServicesCorrectly()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(databaseMock.Object);

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService(typeof(Database)))
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddKeyedCosmosNoSqlVectorStore(serviceKey: "testKey");

            // Assert
            var serviceProvider = serviceProviderMock.Object;
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("testKey");
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("testKey");

            Assert.NotNull(vectorStore);
            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.Same(vectorStore, cosmosNoSqlVectorStore);
        }
    }
}
