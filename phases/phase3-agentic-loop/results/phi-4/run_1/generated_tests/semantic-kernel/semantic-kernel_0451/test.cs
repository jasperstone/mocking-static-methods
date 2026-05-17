using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_CallsGetRequiredServiceForDatabase()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(Database)))
                .Returns(databaseMock.Object);

            // Act
            CosmosNoSqlServiceCollectionExtensions.AddKeyedCosmosNoSqlVectorStore(services, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetService(typeof(Database)), Times.Once);
        }
    }
}
