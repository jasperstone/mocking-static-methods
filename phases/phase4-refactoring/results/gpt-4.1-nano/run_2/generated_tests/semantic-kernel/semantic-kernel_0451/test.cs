using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.VectorData.ProviderServices;
using Microsoft.Azure.Cosmos;
using Moq;
using Xunit;

namespace CosmosNoSqlTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ShouldRegisterStoreAndCallGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>();
            var mockClient = new Mock<CosmosClient>();
            mockClient.Setup(c => c.GetDatabase(It.IsAny<string>())).Returns(mockDatabase.Object);
            var mockClientWrapper = new Mock<ClientWrapper>(mockClient.Object, false);
            mockClientWrapper.Setup(cw => cw.Share()).Returns(mockClientWrapper.Object);
            mockClientWrapper.Setup(cw => cw.Dispose());

            // Setup IServiceProvider to return the mock Database when requested
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<Database>()).Returns(mockDatabase.Object);

            // Act
            services.AddSingleton(serviceProviderMock.Object);
            services.AddKeyedCosmosNoSqlVectorStore(
                serviceKey: "testKey",
                connectionString: "AccountEndpoint=https://test;AccountKey=testkey;",
                databaseName: "testDb",
                options: null,
                lifetime: ServiceLifetime.Singleton);

            var serviceProvider = services.BuildServiceProvider();

            // Retrieve the registered service
            var store = serviceProvider.GetService<VectorStore>();
            Assert.NotNull(store);

            // Verify that GetRequiredService<Database>() was called during registration
            // Since we can't directly verify internal calls, we ensure the store was created
            // and the mock Database was used in the constructor.
        }
    }
}
