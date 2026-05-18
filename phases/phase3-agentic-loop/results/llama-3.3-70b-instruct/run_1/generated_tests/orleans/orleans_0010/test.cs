using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Persistence.DynamoDB;

namespace Orleans.Storage.Tests
{
    public class DynamoDBGrainStorageFactoryTests
    {
        [Fact]
        public void Create_WithValidServiceProvider_ReturnsDynamoDBGrainStorageInstance()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions()
                .Configure<DynamoDBStorageOptions>(options =>
                {
                    options.Service = "test-service";
                    options.AccessKey = "test-access-key";
                    options.SecretKey = "test-secret-key";
                    options.TableName = "test-table-name";
                })
                .BuildServiceProvider();

            // Act
            var dynamoDBGrainStorage = DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name");

            // Assert
            Assert.NotNull(dynamoDBGrainStorage);
        }

        [Fact]
        public void Create_WithInvalidServiceProvider_ThrowsException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProvider, "test-name"));
        }
    }
}
