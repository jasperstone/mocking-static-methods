using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Moq;
using Microsoft.Azure.Cosmos;
using System;
using Microsoft.Extensions.VectorData;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCosmosNoSqlVectorStore_ShouldRegisterServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockDatabase = new Mock<Database>();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(Database))).Returns(mockDatabase.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProvider.Object);

            // Act
            serviceCollection.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProviderBuilt = serviceCollection.BuildServiceProvider();
            var vectorStore = serviceProviderBuilt.GetRequiredService<VectorStore>();
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ShouldRegisterKeyedServices()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockDatabase = new Mock<Database>();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(Database))).Returns(mockDatabase.Object);

            serviceCollection.AddSingleton<IServiceProvider>(serviceProvider.Object);

            // Act
            serviceCollection.AddKeyedCosmosNoSqlVectorStore("key");

            // Assert
            var serviceProviderBuilt = serviceCollection.BuildServiceProvider();
            var vectorStore = serviceProviderBuilt.GetRequiredKeyedService<VectorStore>("key");
            Assert.NotNull(vectorStore);
        }
    }
}
