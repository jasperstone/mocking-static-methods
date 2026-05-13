using System;
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
            var cosmosNoSqlVectorStore = serviceProvider.GetRequiredService<CosmosNoSqlVectorStore>();
            var vectorStore = serviceProvider.GetRequiredService<VectorStore>();

            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ShouldThrowIfDatabaseServiceIsNotRegistered()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns((Database)null);

            serviceCollection.AddSingleton(serviceProviderMock.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddKeyedCosmosNoSqlVectorStore(serviceKey: "testKey"));
        }
    }
}
