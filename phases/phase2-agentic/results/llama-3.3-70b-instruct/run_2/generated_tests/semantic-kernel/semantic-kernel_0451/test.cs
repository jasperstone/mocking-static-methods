using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddKeyedCosmosNoSqlVectorStore_ServiceProvider_GetRequiredService_Database()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosNoSqlVectorStore);
            Assert.Same(databaseMock.Object, cosmosNoSqlVectorStore._database);
        }

        [Fact]
        public async Task AddKeyedCosmosNoSqlVectorStore_ServiceProvider_GetRequiredService_DatabaseAsync()
        {
            // Arrange
            var services = new ServiceCollection();
            var databaseMock = new Mock<Database>();
            services.AddSingleton<Database>(databaseMock.Object);

            // Act
            services.AddKeyedCosmosNoSqlVectorStore(null, null);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var cosmosNoSqlVectorStore = serviceProvider.GetService<CosmosNoSqlVectorStore>();
            Assert.NotNull(cosmosNoSqlVectorStore);
            await Task.CompletedTask;
            Assert.Same(databaseMock.Object, cosmosNoSqlVectorStore._database);
        }
    }
}
