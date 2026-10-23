using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Xunit;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;

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
            var vectorStore = serviceProvider.GetRequiredKeyedService<VectorStore>("testKey");

            Assert.NotNull(vectorStore);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }

        [Fact]
        public void AddCosmosNoSqlVectorStore_ShouldRegisterServices()
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

            Assert.NotNull(vectorStore);
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }
    }
}
