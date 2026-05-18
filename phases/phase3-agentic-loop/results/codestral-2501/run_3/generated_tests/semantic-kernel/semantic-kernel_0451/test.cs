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
        public void AddKeyedCosmosNoSqlVectorStore_ShouldCallGetRequiredService()
        {
            // Arrange
            var serviceCollectionMock = new Mock<IServiceCollection>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();
            var options = new CosmosNoSqlVectorStoreOptions();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(databaseMock.Object);

            // Act
            CosmosNoSqlServiceCollectionExtensions.AddKeyedCosmosNoSqlVectorStore(
                serviceCollectionMock.Object,
                serviceKey: null,
                options: options,
                lifetime: ServiceLifetime.Singleton);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(Database)), Times.Once);
            serviceCollectionMock.Verify(sc => sc.Add(It.IsAny<ServiceDescriptor>()), Times.Exactly(2));
        }
    }
}
