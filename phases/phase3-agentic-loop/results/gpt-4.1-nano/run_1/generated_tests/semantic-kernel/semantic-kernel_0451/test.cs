using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace CosmosNoSqlTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_Should_Register_CosmosNoSqlVectorStore_And_VectorStore()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockClient = new Mock<CosmosClient>();
            mockClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(mockDatabase.Object);
            var mockClientWrapper = new Mock<ClientWrapper>(mockClient.Object, false);
            mockClientWrapper.Setup(cw => cw.Share()).Returns(mockClientWrapper.Object);

            // Register the mock ClientWrapper as singleton
            services.AddSingleton(mockClientWrapper.Object);
            services.AddScoped(sp => new CosmosNoSqlVectorStoreOptions());

            // Act
            var store = new CosmosNoSqlVectorStore(mockClientWrapper.Object, db => mockDatabase.Object, new CosmosNoSqlVectorStoreOptions());

            // Build provider
            var provider = services.BuildServiceProvider();

            // Assert
            var retrievedStore = provider.GetService<CosmosNoSqlVectorStore>();
            var vectorStore = provider.GetService<VectorStore>();

            Assert.NotNull(retrievedStore);
            Assert.NotNull(vectorStore);
        }

        [Fact]
        public void GetCollection_Should_Call_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockClient = new Mock<CosmosClient>();
            mockClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(mockDatabase.Object);
            var mockClientWrapper = new Mock<ClientWrapper>(mockClient.Object, false);
            mockClientWrapper.Setup(cw => cw.Share()).Returns(mockClientWrapper.Object);

            services.AddSingleton(mockClientWrapper.Object);
            services.AddScoped(sp => new CosmosNoSqlVectorStoreOptions());

            var store = new CosmosNoSqlVectorStore(mockClientWrapper.Object, db => mockDatabase.Object, new CosmosNoSqlVectorStoreOptions());

            // Act
            var collection = store.GetCollection<string, Dictionary<string, object?>>("testCollection");

            // Assert
            mockClientWrapper.Verify(cw => cw.Share(), Times.Once);
            Assert.NotNull(collection);
        }
    }
}
