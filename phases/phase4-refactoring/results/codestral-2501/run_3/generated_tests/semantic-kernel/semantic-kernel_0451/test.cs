using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;
using Microsoft.Extensions.VectorData;

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
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProviderMock.Object);

            // Act
            serviceCollection.AddKeyedCosmosNoSqlVectorStore(serviceKey: "testKey", options: null, lifetime: ServiceLifetime.Singleton);

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("testKey");
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("testKey");

            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.NotNull(vectorStore);
            Assert.Same(cosmosNoSqlVectorStore, vectorStore);
        }
    }
}
