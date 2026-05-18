using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.CosmosNoSql;
using System;
using System.Threading.Tasks;
using Xunit;

namespace VectorDataTests
{
    public class CosmosNoSqlServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCosmosNoSqlVectorStore_ServiceProvider_GetRequiredService_Called()
        {
            // Arrange
            var services = new ServiceCollection();
            var cosmosClient = new CosmosClient("DefaultEndpointsProtocol=https;AccountName=<account_name>;AccountKey=<account_key>;BlobEndpoint=<blob_endpoint>");
            var database = cosmosClient.GetDatabase("database");
            services.AddSingleton<Database>(database);

            // Act
            services.AddCosmosNoSqlVectorStore();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<CosmosNoSqlVectorStore>());
        }
    }
}
