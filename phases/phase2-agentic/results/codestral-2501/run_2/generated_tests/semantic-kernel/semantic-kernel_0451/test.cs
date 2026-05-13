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
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddKeyedCosmosNoSqlVectorStore(serviceKey: "testKey");

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredKeyedService<CosmosNoSqlVectorStore>("testKey");
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("testKey");

            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.NotNull(vectorStore);
            Assert.Same(cosmosNoSqlVectorStore, vectorStore);
        }

        [Fact]
        public void AddCosmosNoSqlVectorStore_ShouldCallAddKeyedCosmosNoSqlVectorStore()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(databaseMock.Object);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act
            serviceCollection.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredService<CosmosNoSqlVectorStore>();
            var vectorStore = serviceProvider.GetRequiredService<VectorStore>();

            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.NotNull(vectorStore);
            Assert.Same(cosmosNoSqlVectorStore, vectorStore);
        }
    }
}
