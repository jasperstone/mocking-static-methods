using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddKeyedCosmosNoSqlVectorStore(serviceKey: "testKey");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredService<CosmosNoSqlVectorStore>();

            Assert.NotNull(vectorStore);
            Assert.NotNull(cosmosNoSqlVectorStore);
        }

        [Fact]
        public void AddCosmosNoSqlVectorStore_ShouldCallAddKeyedCosmosNoSqlVectorStore()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredService<CosmosNoSqlVectorStore>();

            Assert.NotNull(vectorStore);
            Assert.NotNull(cosmosNoSqlVectorStore);
        }
    }
}
