using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace VectorData.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCosmosNoSqlVectorStore_ServiceProvider_GetRequiredServiceCalled()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<Database>()).Returns(databaseMock.Object);

            // Act
            services.AddCosmosNoSqlVectorStore(serviceProvider: serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }

        [Fact]
        public async Task AddCosmosNoSqlVectorStore_ServiceProvider_GetRequiredServiceCalledAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseMock = new Mock<Database>();

            serviceProviderMock.Setup(sp => sp.GetRequiredService<Database>()).Returns(databaseMock.Object);

            // Act
            services.AddCosmosNoSqlVectorStore(serviceProvider: serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(sp => sp.GetRequiredService<Database>(), Times.Once);
        }
    }
}
