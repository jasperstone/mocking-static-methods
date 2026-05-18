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
        public void Create_WithValidServiceProvider_ReturnsDynamoDBGrainStorage()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddOptions<DynamoDBStorageOptions>()
                .Configure<DynamoDBStorageOptions>(options =>
                {
                    options.Service = "service";
                    options.AccessKey = "accessKey";
                    options.SecretKey = "secretKey";
                    options.Token = "token";
                    options.ProfileName = "profileName";
                    options.ReadCapacityUnits = 1;
                    options.WriteCapacityUnits = 1;
                    options.UseProvisionedThroughput = true;
                    options.CreateIfNotExists = true;
                    options.UpdateIfExists = true;
                    options.TableName = "tableName";
                    options.ServiceId = "serviceId";
                    options.DeleteStateOnClear = true;
                })
                .BuildServiceProvider();

            var name = "testName";

            // Act
            var dynamoDBGrainStorage = DynamoDBGrainStorageFactory.Create(serviceProvider, name);

            // Assert
            Assert.NotNull(dynamoDBGrainStorage);
        }

        [Fact]
        public void Create_WithInvalidServiceProvider_ThrowsException()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .BuildServiceProvider();

            var name = "testName";

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => DynamoDBGrainStorageFactory.Create(serviceProvider, name));
        }
    }
}
