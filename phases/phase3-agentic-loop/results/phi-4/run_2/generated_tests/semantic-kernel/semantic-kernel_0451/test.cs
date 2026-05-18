using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql; // Added using directive for Database

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_RegistersCosmosNoSqlVectorStore()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock
                .Setup(sp => sp.GetRequiredService<Database>())
                .Returns(databaseMock.Object);

            // Act
            CosmosNoSqlServiceCollectionExtensions.AddKeyedCosmosNoSqlVectorStore(services, null);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }
    }
}
