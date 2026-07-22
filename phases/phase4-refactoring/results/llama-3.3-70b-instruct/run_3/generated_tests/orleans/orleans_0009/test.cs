using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Tests
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_ConfiguresDefaultStorageProvider()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddDynamoDBGrainStorageAsDefault(options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<Orleans.Storage.IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ConfiguresNamedStorageProvider()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";

            // Act
            services.AddDynamoDBGrainStorage(name, options => { });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var grainStorage = serviceProvider.GetService<Orleans.Storage.IGrainStorage>();
            Assert.NotNull(grainStorage);
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ThrowsException_WhenTableNameIsEmpty()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var options = new Orleans.Configuration.DynamoDBStorageOptions { TableName = string.Empty };

            // Act and Assert
            Assert.Throws<OrleansConfigurationException>(() =>
            {
                services.AddDynamoDBGrainStorage(name, ob => ob.Configure(options));
                var serviceProvider = services.BuildServiceProvider();
                var validator = serviceProvider.GetService<IConfigurationValidator>();
                validator.ValidateConfiguration();
            });
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ThrowsException_WhenReadCapacityUnitsIsZero()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var options = new Orleans.Configuration.DynamoDBStorageOptions { ReadCapacityUnits = 0 };

            // Act and Assert
            Assert.Throws<OrleansConfigurationException>(() =>
            {
                services.AddDynamoDBGrainStorage(name, ob => ob.Configure(options));
                var serviceProvider = services.BuildServiceProvider();
                var validator = serviceProvider.GetService<IConfigurationValidator>();
                validator.ValidateConfiguration();
            });
        }

        [Fact]
        public void AddDynamoDBGrainStorage_ThrowsException_WhenWriteCapacityUnitsIsZero()
        {
            // Arrange
            var services = new ServiceCollection();
            var name = "TestStorage";
            var options = new Orleans.Configuration.DynamoDBStorageOptions { WriteCapacityUnits = 0 };

            // Act and Assert
            Assert.Throws<OrleansConfigurationException>(() =>
            {
                services.AddDynamoDBGrainStorage(name, ob => ob.Configure(options));
                var serviceProvider = services.BuildServiceProvider();
                var validator = serviceProvider.GetService<IConfigurationValidator>();
                validator.ValidateConfiguration();
            });
        }
    }
}
