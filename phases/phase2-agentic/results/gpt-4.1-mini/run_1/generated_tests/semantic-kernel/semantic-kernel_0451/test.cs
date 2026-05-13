using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Xunit;
using Moq;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersServices_AndCallsGetRequiredServiceOnServiceProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            var mockDatabase = new Mock<Database>(MockBehavior.Strict, new object[] { null, false });
            var mockServiceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(mockDatabase.Object);
            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(mockDatabase.Object);

            // We will capture the factory delegate to invoke it manually
            ServiceDescriptor? cosmosDescriptor = null;

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(serviceKey: null, options: null, lifetime: ServiceLifetime.Singleton);

            // Find the CosmosNoSqlVectorStore registration
            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType == typeof(CosmosNoSqlVectorStore))
                {
                    cosmosDescriptor = descriptor;
                    break;
                }
            }

            Assert.NotNull(cosmosDescriptor);
            Assert.NotNull(cosmosDescriptor!.ImplementationFactory);

            // Invoke the factory delegate to simulate service provider resolving the service
            var instance = cosmosDescriptor.ImplementationFactory!(mockServiceProvider.Object, null);

            // Assert
            Assert.NotNull(instance);
            Assert.IsType<CosmosNoSqlVectorStore>(instance);

            // Verify that GetRequiredService<Database> was called on the service provider
            mockServiceProvider.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }
    }
}
